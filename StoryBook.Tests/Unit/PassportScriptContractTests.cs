using StoryBook.Tests.Support;

namespace StoryBook.Tests.Unit;

public sealed class PassportScriptContractTests
{
    [Fact]
    public void Passport_script_uses_explicit_click_completion_versioned_state_and_de_duplication()
    {
        string script = File.ReadAllText(Path.Combine(TestPaths.StoryBookRoot, "wwwroot", "js", "passport.js"));

        Assert.Contains("storybook.passport", script);
        Assert.Contains("version: stateVersion", script);
        Assert.Contains("const stateVersion = 1", script);
        Assert.Contains("completedStories", script);
        Assert.Contains("data-passport-story", script);
        Assert.Contains("data-passport-complete", script);
        Assert.Contains("addEventListener(\"click\"", script);
        Assert.Contains("isSameStory", script);
        Assert.Contains("normalizeState", script);
        Assert.Contains("localStorage.setItem", script);
        Assert.DoesNotContain("localStorage.clear", script, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Passport_script_does_not_auto_complete_from_page_load_scroll_or_modals()
    {
        string script = File.ReadAllText(Path.Combine(TestPaths.StoryBookRoot, "wwwroot", "js", "passport.js"));

        Assert.DoesNotContain("addEventListener(\"load\"", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("addEventListener('load'", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("addEventListener(\"scroll\"", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("addEventListener('scroll'", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("data-dino-modal-open", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("data-aquarium-modal-open", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("time-on-page", script, StringComparison.OrdinalIgnoreCase);
    }
}
