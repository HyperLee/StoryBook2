using StoryBook.Tests.Support;

namespace StoryBook.Tests.Unit;

public sealed class JourneysScriptContractTests
{
    [Fact]
    public void Journeys_script_uses_shared_language_key_without_journey_state_or_history_mutation()
    {
        string scriptPath = Path.Combine(TestPaths.StoryBookRoot, "wwwroot", "js", "journeys.js");
        string script = File.ReadAllText(scriptPath);

        Assert.Contains("storybook.language", script);
        Assert.Contains("data-i18n-zh-tw", script);
        Assert.Contains("data-i18n-en", script);
        Assert.Contains("data-language-option", script);
        Assert.DoesNotContain("storybook.journey", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("journeyProgress", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("sessionStorage", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("document.cookie", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("pushState", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("replaceState", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("data-theme-selector", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("storybook.theme", script, StringComparison.OrdinalIgnoreCase);
    }
}
