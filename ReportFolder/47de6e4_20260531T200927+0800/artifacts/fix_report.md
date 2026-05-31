# Fix Report: Spec Kit Scoped Git Staging

## Finding

The validated finding was a local developer-tooling exposure in the Spec Kit Git extension. Automatic hooks and auto-commit can run during Spec Kit workflows, and the Bash and PowerShell auto-commit and initialize scripts used broad worktree staging (`git add .`). That allowed unrelated unignored files in the repository, such as local secrets or diagnostics, to be swept into Git history.

## Fix

The auto-commit and initialize scripts now collect only Spec Kit-owned stage paths:

- `.specify`
- `specs`

The scripts stage those paths with `git add -- ...` and commit with a pathspec (`git commit ... -- ...`) so unrelated untracked or already staged files outside the Spec Kit path set are not included in automated commits. The adjacent command documentation was updated to remove unsafe `git add .` guidance.

## Files Changed

- `.specify/extensions/git/scripts/bash/auto-commit.sh`
- `.specify/extensions/git/scripts/bash/initialize-repo.sh`
- `.specify/extensions/git/scripts/powershell/auto-commit.ps1`
- `.specify/extensions/git/scripts/powershell/initialize-repo.ps1`
- `.specify/extensions/git/commands/speckit.git.commit.md`
- `.specify/extensions/git/commands/speckit.git.initialize.md`
- `StoryBook.Tests/Unit/SpecKitGitScriptSecurityTests.cs`

## Regression Coverage

Added focused xUnit coverage that:

- Runs the Bash auto-commit script in an isolated temp Git repository containing both an expected `specs/001-safe-change/spec.md` file and an unrelated `local-secret.txt` file.
- Runs the Bash initialize script in an isolated temp project with the same safe Spec Kit file and unrelated local file.
- Verifies the expected Spec Kit file is committed and `local-secret.txt` is not committed.
- Checks all four affected executable scripts no longer contain broad `git add .` staging and do use scoped `git add --` staging.

## Validation Evidence

- `dotnet test StoryBook.Tests/StoryBook.Tests.csproj --filter FullyQualifiedName~SpecKitGitScriptSecurityTests`
  - Before the fix: failed as expected; both temp-repo repro tests showed `local-secret.txt` committed, and the static script test found `git add .`.
  - After the fix: passed, 3 passed / 0 failed.
- `dotnet test StoryBook2.sln`
  - Passed, 207 passed / 0 failed.
- `bash -n .specify/extensions/git/scripts/bash/auto-commit.sh`
  - Passed.
- `bash -n .specify/extensions/git/scripts/bash/initialize-repo.sh`
  - Passed.
- `rg -n "git add \\.|Automatically stage and commit all changes|stage and commit all changes" .specify/extensions/git .specify/extensions.yml`
  - No matches.
- `git diff --check`
  - Passed.

## Original Issue No Longer Reproduces

The original issue was reproduced before the fix by executing the vulnerable Bash scripts inside temp Git repositories. In both cases, `local-secret.txt` appeared in `git ls-tree -r --name-only HEAD`.

After the fix, the same temp-repo reproductions pass: `specs/001-safe-change/spec.md` is committed, while `local-secret.txt` remains outside `HEAD` and is still reported by `git status --short -- local-secret.txt` as untracked.

## Remaining Uncertainty

PowerShell runtime validation was not executed because `pwsh` is not installed in this environment (`command -v pwsh` returned exit code 1). The PowerShell scripts were validated by focused static regression coverage and by checking that their staging and commit flow now mirrors the Bash pathspec-based implementation.
