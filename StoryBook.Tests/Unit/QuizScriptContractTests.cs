using StoryBook.Tests.Support;

namespace StoryBook.Tests.Unit;

public sealed class QuizScriptContractTests
{
    [Fact]
    public void Quiz_script_uses_progressive_enhancement_without_storage_network_history_timers_or_correctness_logic()
    {
        string scriptPath = Path.Combine(TestPaths.StoryBookRoot, "wwwroot", "js", "quiz.js");
        string script = File.ReadAllText(scriptPath);

        Assert.Contains("data-quiz-answer-form", script);
        Assert.Contains("data-quiz-feedback", script);
        Assert.DoesNotContain("localStorage", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("sessionStorage", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("document.cookie", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("fetch", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("XMLHttpRequest", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("pushState", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("replaceState", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("jQuery", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("$(", script, StringComparison.Ordinal);
        Assert.DoesNotContain("setTimeout", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("setInterval", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("countdown", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("drag", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("drop", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("pointermove", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("CorrectOptionId", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("isCorrect", script, StringComparison.OrdinalIgnoreCase);
    }
}
