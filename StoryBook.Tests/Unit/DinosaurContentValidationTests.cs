using System.Text.Json;
using StoryBook.Models;
using StoryBook.Services;
using StoryBook.Tests.Support;

namespace StoryBook.Tests.Unit;

public sealed class DinosaurContentValidationTests
{
    [Fact]
    public void Catalog_json_matches_required_shape_and_contains_eight_profiles()
    {
        DinosaurCatalog catalog = LoadCatalog();

        Assert.Equal("1.0", catalog.Version);
        Assert.Equal(8, catalog.Profiles.Count);
        Assert.All(catalog.Profiles, profile =>
        {
            Assert.False(string.IsNullOrWhiteSpace(profile.Slug));
            Assert.Matches("^[a-z0-9]+(?:-[a-z0-9]+)*$", profile.Slug);
            Assert.False(string.IsNullOrWhiteSpace(profile.Names.ZhTW));
            Assert.False(string.IsNullOrWhiteSpace(profile.Names.En));
            Assert.StartsWith("/images/dinosaurs/", profile.MainImage.Path, StringComparison.Ordinal);
            Assert.Equal("storybook-cute", profile.MainImage.StyleTag);
        });
    }

    [Fact]
    public void Catalog_validation_enforces_foundation_content_rules()
    {
        DinosaurCatalog catalog = LoadCatalog();
        DinosaurContentValidator validator = new();

        DinosaurContentValidationResult result = validator.ValidateCatalog(catalog);

        Assert.True(result.IsValid, string.Join(Environment.NewLine, result.Errors));
        Assert.Equal(8, catalog.Profiles.Select(profile => profile.Slug).Distinct(StringComparer.Ordinal).Count());
        Assert.Equal(Enumerable.Range(1, 8), catalog.Profiles.Select(profile => profile.SortOrder).Order());
        Assert.Contains(catalog.Profiles, profile =>
            profile.Slug == "pteranodon"
            && profile.Category == "prehistoric-flying-reptile"
            && !string.IsNullOrWhiteSpace(profile.NotDinosaurNote?.ZhTW)
            && !string.IsNullOrWhiteSpace(profile.NotDinosaurNote?.En));
    }

    [Fact]
    public void Summaries_stories_and_alt_text_fit_bilingual_readability_rules()
    {
        DinosaurCatalog catalog = LoadCatalog();

        foreach (DinosaurProfile profile in catalog.Profiles)
        {
            Assert.InRange(DinosaurContentValidator.CountReadableUnits(profile.Summary.ZhTW, LanguageCode.ZhTW), 1, 200);
            Assert.InRange(DinosaurContentValidator.CountReadableUnits(profile.Summary.En, LanguageCode.En), 1, 200);
            Assert.InRange(DinosaurContentValidator.CountReadableUnits(profile.Story.Body.ZhTW, LanguageCode.ZhTW), 100, 150);
            Assert.InRange(DinosaurContentValidator.CountReadableUnits(profile.Story.Body.En, LanguageCode.En), 100, 150);
            Assert.NotEqual(Path.GetFileName(profile.MainImage.Path), profile.MainImage.AltText.ZhTW);
            Assert.NotEqual(Path.GetFileName(profile.Story.Illustration.Path), profile.Story.Illustration.AltText.En);
            Assert.False(string.IsNullOrWhiteSpace(profile.MainImage.AltText.En));
            Assert.False(string.IsNullOrWhiteSpace(profile.Story.Illustration.AltText.ZhTW));
        }
    }

    [Fact]
    public void Stories_keep_warm_tone_and_storybook_illustration_style()
    {
        DinosaurCatalog catalog = LoadCatalog();
        string[] disallowedTone = ["血", "殺", "嚇", "scary", "kill", "blood"];
        string[] warmToneMarkers = ["朋友", "大家", "小恐龍", "溫柔", "分享", "友情", "幫"];

        foreach (DinosaurProfile profile in catalog.Profiles)
        {
            string zhStory = profile.Story.Body.ZhTW;
            string enStory = profile.Story.Body.En;

            Assert.Contains(warmToneMarkers, marker => zhStory.Contains(marker, StringComparison.OrdinalIgnoreCase));
            Assert.DoesNotContain(disallowedTone, word => zhStory.Contains(word, StringComparison.OrdinalIgnoreCase));
            Assert.DoesNotContain(disallowedTone, word => enStory.Contains(word, StringComparison.OrdinalIgnoreCase));
            Assert.Equal("storybook-cute", profile.Story.Illustration.StyleTag);
        }
    }

    private static DinosaurCatalog LoadCatalog()
    {
        string path = Path.Combine(TestPaths.StoryBookRoot, "Data", "dinosaurs.json");
        string json = File.ReadAllText(path);
        DinosaurCatalog? catalog = JsonSerializer.Deserialize<DinosaurCatalog>(json, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        return Assert.IsType<DinosaurCatalog>(catalog);
    }
}
