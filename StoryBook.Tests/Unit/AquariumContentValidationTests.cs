using System.Text.Json;
using StoryBook.Models;
using StoryBook.Services;
using StoryBook.Tests.Support;

namespace StoryBook.Tests.Unit;

public sealed class AquariumContentValidationTests
{
    private static readonly string[] ExpectedSlugs =
    [
        "clownfish",
        "seahorse",
        "sea-turtle",
        "jellyfish",
        "octopus",
        "shark",
        "stingray",
        "penguin",
        "seal",
        "dolphin",
        "starfish",
        "crab",
        "coral",
        "goldfish",
        "axolotl"
    ];

    [Fact]
    public void Catalog_json_matches_required_shape_and_fixed_animal_set()
    {
        AquariumCatalog catalog = LoadCatalog();

        Assert.False(string.IsNullOrWhiteSpace(catalog.Version));
        Assert.Equal(ExpectedSlugs, catalog.Animals.OrderBy(profile => profile.SortOrder).Select(profile => profile.Slug));
        Assert.Equal(15, catalog.Animals.Count);
        Assert.True(catalog.HabitatCategories.Count >= 5);
        Assert.All(catalog.HabitatCategories, category =>
        {
            Assert.Matches("^[a-z0-9]+(?:-[a-z0-9]+)*$", category.Code);
            Assert.False(string.IsNullOrWhiteSpace(category.DisplayName.ZhTW));
            Assert.False(string.IsNullOrWhiteSpace(category.DisplayName.En));
        });
    }

    [Fact]
    public void Catalog_validation_enforces_bilingual_content_and_schema_aligned_paths()
    {
        AquariumCatalog catalog = LoadCatalog();
        AquariumContentValidator validator = new();

        AquariumContentValidationResult result = validator.ValidateCatalog(catalog);

        Assert.True(result.IsValid, string.Join(Environment.NewLine, result.Errors));
        Assert.Equal(15, catalog.Animals.Select(profile => profile.Slug).Distinct(StringComparer.Ordinal).Count());
        Assert.Equal(Enumerable.Range(1, 15), catalog.Animals.Select(profile => profile.SortOrder).Order());
        Assert.All(catalog.Animals, profile =>
        {
            Assert.Contains(profile.HabitatCategory, catalog.HabitatCategories.Select(category => category.Code));
            Assert.StartsWith("/images/aquarium/", profile.MainImage.Path, StringComparison.Ordinal);
            Assert.StartsWith("/images/aquarium/", profile.Story.Illustration.Path, StringComparison.Ordinal);
            Assert.Equal("storybook-cute", profile.MainImage.StyleTag);
            Assert.Equal("storybook-cute", profile.Story.Illustration.StyleTag);
        });
    }

    [Fact]
    public void Summaries_stories_and_alt_text_fit_bilingual_readability_rules()
    {
        AquariumCatalog catalog = LoadCatalog();

        foreach (AquariumAnimalProfile profile in catalog.Animals)
        {
            Assert.InRange(AquariumContentValidator.CountReadableUnits(profile.Summary.ZhTW, LanguageCode.ZhTW), 1, 200);
            Assert.InRange(AquariumContentValidator.CountReadableUnits(profile.Summary.En, LanguageCode.En), 1, 200);
            Assert.InRange(AquariumContentValidator.CountReadableUnits(profile.Story.Body.ZhTW, LanguageCode.ZhTW), 100, 150);
            Assert.InRange(AquariumContentValidator.CountReadableUnits(profile.Story.Body.En, LanguageCode.En), 100, 150);
            Assert.NotEqual(Path.GetFileName(profile.MainImage.Path), profile.MainImage.AltText.ZhTW);
            Assert.NotEqual(Path.GetFileName(profile.MainImage.Path), profile.MainImage.AltText.En);
            Assert.NotEqual(Path.GetFileName(profile.Story.Illustration.Path), profile.Story.Illustration.AltText.ZhTW);
            Assert.NotEqual(Path.GetFileName(profile.Story.Illustration.Path), profile.Story.Illustration.AltText.En);
            Assert.False(string.IsNullOrWhiteSpace(profile.MainImage.AltText.ZhTW));
            Assert.False(string.IsNullOrWhiteSpace(profile.MainImage.AltText.En));
            Assert.False(string.IsNullOrWhiteSpace(profile.Story.Illustration.AltText.ZhTW));
            Assert.False(string.IsNullOrWhiteSpace(profile.Story.Illustration.AltText.En));
        }
    }

    private static AquariumCatalog LoadCatalog()
    {
        string path = Path.Combine(TestPaths.StoryBookRoot, "Data", "aquarium.json");
        string json = File.ReadAllText(path);
        AquariumCatalog? catalog = JsonSerializer.Deserialize<AquariumCatalog>(json, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        return Assert.IsType<AquariumCatalog>(catalog);
    }
}
