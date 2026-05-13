namespace StoryBook.Tests.Integration;

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
