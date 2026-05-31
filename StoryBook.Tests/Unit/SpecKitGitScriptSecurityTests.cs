using System.Diagnostics;
using StoryBook.Tests.Support;

namespace StoryBook.Tests.Unit;

public sealed class SpecKitGitScriptSecurityTests
{
    private static readonly string BashScriptDirectory = Path.Combine(
        TestPaths.RepositoryRoot,
        ".specify",
        "extensions",
        "git",
        "scripts",
        "bash");

    private static readonly string PowerShellScriptDirectory = Path.Combine(
        TestPaths.RepositoryRoot,
        ".specify",
        "extensions",
        "git",
        "scripts",
        "powershell");

    [Fact]
    public void Auto_commit_commits_spec_kit_changes_without_committing_unrelated_untracked_files()
    {
        string projectRoot = CreateSpecKitProjectWithBashScript("auto-commit.sh");

        try
        {
            InitializeRepository(projectRoot);

            string specPath = Path.Combine(projectRoot, "specs", "001-safe-change", "spec.md");
            Directory.CreateDirectory(Path.GetDirectoryName(specPath)!);
            File.WriteAllText(specPath, "# Safe Spec Kit change" + Environment.NewLine);
            File.WriteAllText(Path.Combine(projectRoot, "local-secret.txt"), "do not commit" + Environment.NewLine);

            CommandResult result = Run(
                "bash",
                projectRoot,
                Path.Combine(projectRoot, ".specify", "extensions", "git", "scripts", "bash", "auto-commit.sh"),
                "after_specify");

            AssertCommandSucceeded(result);
            string committedFiles = RunGit(projectRoot, "ls-tree", "-r", "--name-only", "HEAD").StandardOutput;
            Assert.Contains("specs/001-safe-change/spec.md", committedFiles);
            Assert.DoesNotContain("local-secret.txt", committedFiles);

            string secretStatus = RunGit(projectRoot, "status", "--short", "--", "local-secret.txt").StandardOutput;
            Assert.StartsWith("?? local-secret.txt", secretStatus, StringComparison.Ordinal);
        }
        finally
        {
            DeleteDirectoryIfExists(projectRoot);
        }
    }

    [Fact]
    public void Initialize_repo_commits_spec_kit_paths_without_committing_unrelated_untracked_files()
    {
        string projectRoot = CreateSpecKitProjectWithBashScript("initialize-repo.sh");

        try
        {
            string specPath = Path.Combine(projectRoot, "specs", "001-safe-change", "spec.md");
            Directory.CreateDirectory(Path.GetDirectoryName(specPath)!);
            File.WriteAllText(specPath, "# Safe Spec Kit change" + Environment.NewLine);
            File.WriteAllText(Path.Combine(projectRoot, "local-secret.txt"), "do not commit" + Environment.NewLine);

            CommandResult initResult = Run(
                "bash",
                projectRoot,
                Path.Combine(projectRoot, ".specify", "extensions", "git", "scripts", "bash", "initialize-repo.sh"));

            AssertCommandSucceeded(initResult);
            ConfigureGitIdentity(projectRoot);

            string committedFiles = RunGit(projectRoot, "ls-tree", "-r", "--name-only", "HEAD").StandardOutput;
            Assert.Contains(".specify/extensions/git/git-config.yml", committedFiles);
            Assert.Contains("specs/001-safe-change/spec.md", committedFiles);
            Assert.DoesNotContain("local-secret.txt", committedFiles);

            string secretStatus = RunGit(projectRoot, "status", "--short", "--", "local-secret.txt").StandardOutput;
            Assert.StartsWith("?? local-secret.txt", secretStatus, StringComparison.Ordinal);
        }
        finally
        {
            DeleteDirectoryIfExists(projectRoot);
        }
    }

    [Fact]
    public void Spec_kit_git_scripts_do_not_use_broad_worktree_staging()
    {
        string[] scriptPaths =
        [
            Path.Combine(BashScriptDirectory, "auto-commit.sh"),
            Path.Combine(BashScriptDirectory, "initialize-repo.sh"),
            Path.Combine(PowerShellScriptDirectory, "auto-commit.ps1"),
            Path.Combine(PowerShellScriptDirectory, "initialize-repo.ps1")
        ];

        foreach (string scriptPath in scriptPaths)
        {
            string script = File.ReadAllText(scriptPath);

            Assert.DoesNotContain("git add .", script, StringComparison.Ordinal);
            Assert.Contains("git add --", script, StringComparison.Ordinal);
        }
    }

    private static string CreateSpecKitProjectWithBashScript(string scriptName)
    {
        string projectRoot = Path.Combine(
            Path.GetTempPath(),
            "storybook-speckit-git-" + Guid.NewGuid().ToString("N"));
        string bashTargetDirectory = Path.Combine(
            projectRoot,
            ".specify",
            "extensions",
            "git",
            "scripts",
            "bash");
        string configTargetDirectory = Path.Combine(projectRoot, ".specify", "extensions", "git");

        Directory.CreateDirectory(bashTargetDirectory);
        File.Copy(Path.Combine(BashScriptDirectory, scriptName), Path.Combine(bashTargetDirectory, scriptName));
        Directory.CreateDirectory(configTargetDirectory);
        File.WriteAllText(
            Path.Combine(configTargetDirectory, "git-config.yml"),
            """
            init_commit_message: "[Spec Kit] Initial commit"
            auto_commit:
              default: true
              after_specify:
                enabled: true
                message: "[Spec Kit] Add specification"
            """);

        return projectRoot;
    }

    private static void InitializeRepository(string projectRoot)
    {
        RunGit(projectRoot, "init", "-q");
        ConfigureGitIdentity(projectRoot);
        RunGit(projectRoot, "add", "--", ".specify");
        RunGit(projectRoot, "commit", "-q", "-m", "baseline");
    }

    private static void ConfigureGitIdentity(string projectRoot)
    {
        RunGit(projectRoot, "config", "user.email", "spec-kit-test@example.invalid");
        RunGit(projectRoot, "config", "user.name", "Spec Kit Test");
    }

    private static CommandResult RunGit(string workingDirectory, params string[] arguments)
    {
        CommandResult result = Run("git", workingDirectory, arguments);
        AssertCommandSucceeded(result);
        return result;
    }

    private static CommandResult Run(string fileName, string workingDirectory, params string[] arguments)
    {
        ProcessStartInfo startInfo = new()
        {
            FileName = fileName,
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };
        startInfo.Environment["GIT_AUTHOR_EMAIL"] = "spec-kit-test@example.invalid";
        startInfo.Environment["GIT_AUTHOR_NAME"] = "Spec Kit Test";
        startInfo.Environment["GIT_COMMITTER_EMAIL"] = "spec-kit-test@example.invalid";
        startInfo.Environment["GIT_COMMITTER_NAME"] = "Spec Kit Test";

        foreach (string argument in arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

        using Process process = Process.Start(startInfo)
            ?? throw new InvalidOperationException($"Failed to start {fileName}.");
        Task<string> standardOutput = process.StandardOutput.ReadToEndAsync();
        Task<string> standardError = process.StandardError.ReadToEndAsync();

        if (!process.WaitForExit(milliseconds: 30_000))
        {
            process.Kill(entireProcessTree: true);
            throw new TimeoutException($"{fileName} did not exit within 30 seconds.");
        }

        return new CommandResult(
            process.ExitCode,
            standardOutput.GetAwaiter().GetResult(),
            standardError.GetAwaiter().GetResult());
    }

    private static void AssertCommandSucceeded(CommandResult result)
    {
        Assert.True(
            result.ExitCode == 0,
            $"""
            Expected command to succeed.
            Exit code: {result.ExitCode}
            STDOUT:
            {result.StandardOutput}
            STDERR:
            {result.StandardError}
            """);
    }

    private static void DeleteDirectoryIfExists(string path)
    {
        if (Directory.Exists(path))
        {
            Directory.Delete(path, recursive: true);
        }
    }

    private sealed record CommandResult(int ExitCode, string StandardOutput, string StandardError);
}
