namespace StoryBook.Tests.Integration;

using StoryBook.Tests.Support;

public sealed class ThemePagesTests : IClassFixture<DinosaurPageTestFixture>
{
    private readonly DinosaurPageTestFixture _fixture;

    public ThemePagesTests(DinosaurPageTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Home_page_renders_exactly_one_accessible_theme_selector()
    {
        string html = await _fixture.GetOkHtmlAsync("/");

        Assert.Equal(1, CountOccurrences(html, "data-theme-selector"));
        Assert.Contains("data-theme-storage-key=\"storybook.theme\"", html);
        Assert.Contains("aria-labelledby=\"theme-selector-title\"", html);
        Assert.Contains("data-theme-option=\"light\"", html);
        Assert.Contains("data-theme-option=\"dark\"", html);
        Assert.Contains("data-theme-option=\"system\"", html);
        Assert.Equal(3, CountOccurrences(html, "data-theme-option="));
    }

    [Theory]
    [InlineData("/Privacy")]
    [InlineData("/Error")]
    [InlineData("/dinosaurs")]
    [InlineData("/dinosaurs/tyrannosaurus-rex")]
    [InlineData("/aquarium")]
    [InlineData("/aquarium/clownfish")]
    public async Task Non_home_routes_do_not_render_theme_selector(string route)
    {
        string html = await _fixture.GetOkHtmlAsync(route);

        AssertThemeSelectorAbsent(html);
    }

    [Fact]
    public async Task Shared_layout_emits_early_theme_boot_script_before_main_stylesheets()
    {
        string html = await _fixture.GetOkHtmlAsync("/");

        int bootScriptIndex = html.IndexOf("data-storybook-theme-boot", StringComparison.OrdinalIgnoreCase);
        int firstStylesheetIndex = html.IndexOf("<link rel=\"stylesheet\"", StringComparison.OrdinalIgnoreCase);

        Assert.True(bootScriptIndex >= 0, "Expected the layout to include the early theme boot script.");
        Assert.True(firstStylesheetIndex >= 0, "Expected the layout to include stylesheet links.");
        Assert.True(bootScriptIndex < firstStylesheetIndex, "The theme boot script must run before main stylesheets.");
        AssertThemeAttributeContract(html);
        Assert.Contains("data-storybook-theme-mode=\"system\"", html);
        Assert.Contains("data-storybook-effective-theme=\"light\"", html);
        Assert.Contains("storybook.theme", html);
    }

    [Fact]
    public async Task Home_theme_selector_exposes_bilingual_text_and_language_fallback_contract()
    {
        string html = await _fixture.GetOkHtmlAsync("/");

        Assert.Contains("data-theme-language-storage-key=\"storybook.language\"", html);
        Assert.Contains("data-theme-language-fallback=\"zh-TW\"", html);
        Assert.Contains("data-theme-label-zh-tw=\"亮色模式\"", html);
        Assert.Contains("data-theme-label-en=\"Light mode\"", html);
        Assert.Contains("data-theme-label-zh-tw=\"深色模式\"", html);
        Assert.Contains("data-theme-label-en=\"Dark mode\"", html);
        Assert.Contains("data-theme-label-zh-tw=\"跟隨系統\"", html);
        Assert.Contains("data-theme-label-en=\"Use system setting\"", html);
        Assert.Equal(3, CountOccurrences(html, "data-theme-description-zh-tw="));
        Assert.Equal(3, CountOccurrences(html, "data-theme-description-en="));
        Assert.Contains("data-theme-selected-status", html);
        Assert.DoesNotContain("data-theme-label-zh-tw=\"\"", html);
        Assert.DoesNotContain("data-theme-description-zh-tw=\"\"", html);
    }

    [Theory]
    [InlineData("/", "/dinosaurs", "/aquarium")]
    [InlineData("/Privacy", "/", "/Privacy")]
    [InlineData("/Error", "/", "/Privacy")]
    [InlineData("/dinosaurs", "/", "/dinosaurs/tyrannosaurus-rex")]
    [InlineData("/dinosaurs/tyrannosaurus-rex", "/", "/dinosaurs")]
    [InlineData("/aquarium", "/", "/aquarium/clownfish")]
    [InlineData("/aquarium/clownfish", "/", "/aquarium")]
    public async Task Themed_routes_keep_canonical_anchors_and_theme_assets(string route, string firstHref, string secondHref)
    {
        string html = await _fixture.GetOkHtmlAsync(route);

        Assert.Contains("data-storybook-theme-boot", html);
        Assert.Contains("/js/theme.js", html);
        AssertThemeAttributeContract(html);
        Assert.True(DinosaurPageTestFixture.HasLinkTo(html, firstHref), $"Expected {route} to link to {firstHref}.");
        Assert.True(DinosaurPageTestFixture.HasLinkTo(html, secondHref), $"Expected {route} to link to {secondHref}.");
        AssertNoThemeNavigationArtifacts(html);
    }

    [Fact]
    public void Theme_script_syncs_storage_without_router_or_jquery_dependency()
    {
        string scriptPath = Path.Combine(TestPaths.StoryBookRoot, "wwwroot", "js", "theme.js");
        string script = File.ReadAllText(scriptPath);

        Assert.Contains("addEventListener(\"storage\"", script);
        Assert.Contains("event.key", script);
        Assert.Contains("scrollX", script);
        Assert.Contains("scrollY", script);
        Assert.DoesNotContain("pushState", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("replaceState", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("location.hash", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("data-js-router", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("jquery", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("$(", script, StringComparison.OrdinalIgnoreCase);
    }

    private static void AssertThemeAttributeContract(string html)
    {
        Assert.Contains("data-bs-theme", html);
        Assert.Contains("data-storybook-theme-mode", html);
        Assert.Contains("data-storybook-effective-theme", html);
    }

    private static void AssertThemeSelectorAbsent(string html)
    {
        Assert.DoesNotContain("data-theme-selector", html, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("data-theme-option=", html, StringComparison.OrdinalIgnoreCase);
    }

    private static void AssertNoThemeNavigationArtifacts(string html)
    {
        Assert.DoesNotContain("?theme=", html, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("#theme", html, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("pushState", html, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("replaceState", html, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("location.hash", html, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("data-js-router", html, StringComparison.OrdinalIgnoreCase);
    }

    private static int CountOccurrences(string value, string search)
    {
        int count = 0;
        int index = 0;

        while ((index = value.IndexOf(search, index, StringComparison.OrdinalIgnoreCase)) >= 0)
        {
            count++;
            index += search.Length;
        }

        return count;
    }
}
