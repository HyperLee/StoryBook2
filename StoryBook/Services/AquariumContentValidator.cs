using System.Text.RegularExpressions;
using StoryBook.Models;

namespace StoryBook.Services;

/// <summary>
/// Validates local aquarium catalog content before it is displayed.
/// </summary>
public sealed partial class AquariumContentValidator
{
    private static readonly HashSet<string> ExpectedSlugs =
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

    private static readonly HashSet<string> AllowedHabitatCategories =
    [
        "freshwater",
        "saltwater",
        "deep-sea",
        "coral-reef",
        "polar",
        "tide-pool"
    ];

    /// <summary>
    /// Validates the complete catalog against the feature content rules.
    /// </summary>
    public AquariumContentValidationResult ValidateCatalog(AquariumCatalog? catalog)
    {
        List<string> errors = [];

        if (catalog is null)
        {
            errors.Add("Catalog is missing.");
            return new AquariumContentValidationResult(errors);
        }

        if (string.IsNullOrWhiteSpace(catalog.Version))
        {
            errors.Add("Catalog version is required.");
        }

        if (catalog.HabitatCategories.Count < 5)
        {
            errors.Add("Catalog must contain at least 5 habitat categories.");
        }

        if (catalog.Animals.Count != 15)
        {
            errors.Add("Catalog must contain exactly 15 aquarium animals.");
        }

        ValidateUniqueValues(catalog.HabitatCategories, category => category.Code, "habitat category code", errors);
        ValidateUniqueValues(catalog.HabitatCategories, category => category.SortOrder, "habitat category sortOrder", errors);
        ValidateUniqueValues(catalog.Animals, profile => profile.Slug, "slug", errors);
        ValidateUniqueValues(catalog.Animals, profile => profile.SortOrder, "sortOrder", errors);

        HashSet<string> categoryCodes = catalog.HabitatCategories.Select(category => category.Code).ToHashSet(StringComparer.Ordinal);

        foreach (AquariumHabitatCategory category in catalog.HabitatCategories)
        {
            ValidateCategory(category, errors);
        }

        foreach (AquariumAnimalProfile profile in catalog.Animals)
        {
            ValidateProfile(profile, categoryCodes, errors);
        }

        foreach (string expectedSlug in ExpectedSlugs)
        {
            if (!catalog.Animals.Any(profile => profile.Slug == expectedSlug))
            {
                errors.Add($"{expectedSlug}: required aquarium animal is missing.");
            }
        }

        return new AquariumContentValidationResult(errors);
    }

    /// <summary>
    /// Counts readable units. English uses words; Traditional Chinese uses visible letters and numbers.
    /// </summary>
    public static int CountReadableUnits(string? text, LanguageCode language)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return 0;
        }

        if (language == LanguageCode.En)
        {
            return WordRegex().Matches(text).Count;
        }

        return text.Count(char.IsLetterOrDigit);
    }

    private static void ValidateCategory(AquariumHabitatCategory category, List<string> errors)
    {
        if (!SlugRegex().IsMatch(category.Code))
        {
            errors.Add($"{category.Code}: habitat category code must be lowercase kebab-case.");
        }

        if (!AllowedHabitatCategories.Contains(category.Code))
        {
            errors.Add($"{category.Code}: habitat category is not allowed by the content schema.");
        }

        if (category.SortOrder is < 1 or > 10)
        {
            errors.Add($"{category.Code}: habitat category sortOrder must be 1-10.");
        }

        ValidateText(category.Code, "displayName", category.DisplayName, errors);

        if (category.Description is not null)
        {
            ValidateText(category.Code, "description", category.Description, errors);
        }
    }

    private static void ValidateProfile(AquariumAnimalProfile profile, HashSet<string> categoryCodes, List<string> errors)
    {
        if (!SlugRegex().IsMatch(profile.Slug))
        {
            errors.Add($"{profile.Slug}: slug must be lowercase kebab-case.");
        }

        if (!ExpectedSlugs.Contains(profile.Slug))
        {
            errors.Add($"{profile.Slug}: slug is not in the fixed aquarium animal set.");
        }

        if (!categoryCodes.Contains(profile.HabitatCategory))
        {
            errors.Add($"{profile.Slug}: habitatCategory must match an existing category.");
        }

        if (profile.SortOrder is < 1 or > 15)
        {
            errors.Add($"{profile.Slug}: sortOrder must be 1-15.");
        }

        ValidateText(profile.Slug, "names", profile.Names, errors);
        ValidateText(profile.Slug, "habitat", profile.Habitat, errors);
        ValidateText(profile.Slug, "diet", profile.Diet, errors);
        ValidateText(profile.Slug, "discoveryLocations", profile.DiscoveryLocations, errors);
        ValidateText(profile.Slug, "summary", profile.Summary, errors);
        ValidateReadableLimit(profile.Slug, "summary.zhTW", profile.Summary.ZhTW, LanguageCode.ZhTW, 1, 200, errors);
        ValidateReadableLimit(profile.Slug, "summary.en", profile.Summary.En, LanguageCode.En, 1, 200, errors);
        ValidateImage(profile.Slug, "mainImage", profile.MainImage, errors);
        ValidateStory(profile, errors);

        if (profile.SearchKeywords.Count == 0)
        {
            errors.Add($"{profile.Slug}: searchKeywords must not be empty.");
        }

        foreach (AquariumText keyword in profile.SearchKeywords)
        {
            ValidateText(profile.Slug, "searchKeywords", keyword, errors);
        }
    }

    private static void ValidateStory(AquariumAnimalProfile profile, List<string> errors)
    {
        ValidateText(profile.Slug, "story.title", profile.Story.Title, errors);
        ValidateText(profile.Slug, "story.body", profile.Story.Body, errors);
        ValidateReadableLimit(profile.Slug, "story.body.zhTW", profile.Story.Body.ZhTW, LanguageCode.ZhTW, 100, 150, errors);
        ValidateReadableLimit(profile.Slug, "story.body.en", profile.Story.Body.En, LanguageCode.En, 100, 150, errors);
        ValidateImage(profile.Slug, "story.illustration", profile.Story.Illustration, errors);
    }

    private static void ValidateImage(string slug, string field, AquariumImage image, List<string> errors)
    {
        if (!ImagePathRegex().IsMatch(image.Path))
        {
            errors.Add($"{slug}: {field}.path must be a local aquarium image path.");
        }

        if (image.StyleTag != "storybook-cute")
        {
            errors.Add($"{slug}: {field}.styleTag must be storybook-cute.");
        }

        ValidateText(slug, $"{field}.altText", image.AltText, errors);

        string fileName = Path.GetFileName(image.Path);

        if (string.Equals(image.AltText.ZhTW, fileName, StringComparison.OrdinalIgnoreCase)
            || string.Equals(image.AltText.En, fileName, StringComparison.OrdinalIgnoreCase))
        {
            errors.Add($"{slug}: {field}.altText must not be the file name.");
        }

        if (image.Caption is not null)
        {
            ValidateText(slug, $"{field}.caption", image.Caption, errors);
        }
    }

    private static void ValidateText(string slug, string field, AquariumText text, List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(text.ZhTW))
        {
            errors.Add($"{slug}: {field}.zhTW is required.");
        }

        if (string.IsNullOrWhiteSpace(text.En))
        {
            errors.Add($"{slug}: {field}.en is required.");
        }
    }

    private static void ValidateReadableLimit(
        string slug,
        string field,
        string text,
        LanguageCode language,
        int minimum,
        int maximum,
        List<string> errors)
    {
        int count = CountReadableUnits(text, language);

        if (count < minimum || count > maximum)
        {
            errors.Add($"{slug}: {field} must be between {minimum} and {maximum} readable units, but was {count}.");
        }
    }

    private static void ValidateUniqueValues<TItem, TValue>(
        IReadOnlyList<TItem> items,
        Func<TItem, TValue> selector,
        string field,
        List<string> errors)
        where TValue : notnull
    {
        HashSet<TValue> values = [];

        foreach (TItem item in items)
        {
            TValue value = selector(item);

            if (!values.Add(value))
            {
                errors.Add($"Duplicate {field}: {value}.");
            }
        }
    }

    [GeneratedRegex("^[a-z0-9]+(?:-[a-z0-9]+)*$")]
    private static partial Regex SlugRegex();

    [GeneratedRegex("^/images/aquarium/[a-z0-9-]+\\.(png|jpg|jpeg|webp)$", RegexOptions.IgnoreCase)]
    private static partial Regex ImagePathRegex();

    [GeneratedRegex("[\\p{L}\\p{N}]+")]
    private static partial Regex WordRegex();
}
