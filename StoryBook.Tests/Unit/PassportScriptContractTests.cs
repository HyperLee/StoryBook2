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

    [Fact]
    public void Passport_script_resets_only_passport_state_after_clear_confirmation()
    {
        string script = File.ReadAllText(Path.Combine(TestPaths.StoryBookRoot, "wwwroot", "js", "passport.js"));

        Assert.Contains("data-passport-clear", script);
        Assert.Contains("data-passport-clear-confirm", script);
        Assert.Contains("data-passport-clear-confirm-action", script);
        Assert.Contains("data-passport-clear-cancel", script);
        Assert.Contains("resetPassportState", script);
        Assert.Contains("writePassportState(createEmptyState())", script);
        Assert.Contains("completedStories: []", script);
        Assert.DoesNotContain("localStorage.clear", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("removeItem(languageStorageKey", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("removeItem(\"storybook.language\"", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("storybook.theme", script, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Passport_script_handles_storage_blocking_invalid_data_and_forbidden_fallbacks()
    {
        string script = File.ReadAllText(Path.Combine(TestPaths.StoryBookRoot, "wwwroot", "js", "passport.js"));

        Assert.Contains("try", script);
        Assert.Contains("catch", script);
        Assert.Contains("localStorage.getItem", script);
        Assert.Contains("localStorage.setItem", script);
        Assert.Contains("read-blocked", script);
        Assert.Contains("write-blocked", script);
        Assert.Contains("invalid-data", script);
        Assert.Contains("ignoredItemCount", script);
        Assert.Contains("normalizeCompletedItem", script);
        Assert.Contains("knownStories instanceof Map", script);
        Assert.Contains("allowedSources.includes", script);
        Assert.Contains("slugPattern.test", script);
        Assert.DoesNotContain("sessionStorage", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("document.cookie", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("fetch(", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("XMLHttpRequest", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("pushState", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("replaceState", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("location.search", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("removeItem", script, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Passport_static_assets_have_no_jquery_router_inline_or_external_dependencies_and_cover_focus_responsive_states()
    {
        string script = File.ReadAllText(Path.Combine(TestPaths.StoryBookRoot, "wwwroot", "js", "passport.js"));
        string css = File.ReadAllText(Path.Combine(TestPaths.StoryBookRoot, "wwwroot", "css", "passport.css"));

        Assert.Contains("data-passport-page", script);
        Assert.Contains("data-aria-label-zh-tw", script);
        Assert.Contains("data-i18n-zh-tw", script);
        Assert.DoesNotContain("jQuery", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("$(", script, StringComparison.Ordinal);
        Assert.DoesNotContain("pushState", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("replaceState", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("location.hash", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("http://", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("https://", script, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(":focus-visible", css);
        Assert.Contains("@media (max-width: 575.98px)", css);
        Assert.Contains("min-height: 44px", css);
        Assert.Contains("var(--storybook", css);
    }
}
