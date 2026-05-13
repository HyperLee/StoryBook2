namespace StoryBook.Tests.Integration;

public sealed class ThemePagesTests : IClassFixture<DinosaurPageTestFixture>
{
    private readonly DinosaurPageTestFixture _fixture;

    public ThemePagesTests(DinosaurPageTestFixture fixture)
    {
        _fixture = fixture;
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
