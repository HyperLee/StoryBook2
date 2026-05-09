using System.Text.RegularExpressions;
using StoryBook.Models;

namespace StoryBook.Services;

/// <summary>
/// Validates local dinosaur catalog content before it is displayed.
/// </summary>
public sealed partial class DinosaurContentValidator
{
    private static readonly HashSet<string> AllowedCategories = ["dinosaur", "prehistoric-flying-reptile"];

    /// <summary>
    /// Validates the complete catalog against the feature content rules.
    /// </summary>
    public DinosaurContentValidationResult ValidateCatalog(DinosaurCatalog? catalog)
    {
        List<string> errors = [];

        if (catalog is null)
        {
            errors.Add("Catalog is missing.");
            return new DinosaurContentValidationResult(errors);
        }

        if (string.IsNullOrWhiteSpace(catalog.Version))
        {
            errors.Add("Catalog version is required.");
        }

        if (catalog.Profiles.Count != 8)
        {
            errors.Add("Catalog must contain exactly 8 profiles.");
        }

        ValidateUniqueValues(catalog.Profiles, profile => profile.Slug, "slug", errors);
        ValidateUniqueValues(catalog.Profiles, profile => profile.SortOrder, "sortOrder", errors);

        foreach (DinosaurProfile profile in catalog.Profiles)
        {
            ValidateProfile(profile, errors);
        }

        DinosaurProfile? pteranodon = catalog.Profiles.FirstOrDefault(profile => profile.Slug == "pteranodon");

        if (pteranodon is null)
        {
            errors.Add("Pteranodon profile is required.");
        }
        else if (pteranodon.Category != "prehistoric-flying-reptile")
        {
            errors.Add("Pteranodon must be categorized as prehistoric-flying-reptile.");
        }

        return new DinosaurContentValidationResult(errors);
    }

    /// <summary>
    /// Counts readable units. English uses words; Traditional Chinese uses visible non-whitespace characters.
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

        return text.Count(character => !char.IsWhiteSpace(character));
    }

    private static void ValidateProfile(DinosaurProfile profile, List<string> errors)
    {
        if (!SlugRegex().IsMatch(profile.Slug))
        {
            errors.Add($"{profile.Slug}: slug must be lowercase kebab-case.");
        }

        if (!AllowedCategories.Contains(profile.Category))
        {
            errors.Add($"{profile.Slug}: category is invalid.");
        }

        if (profile.SortOrder is < 1 or > 8)
        {
            errors.Add($"{profile.Slug}: sortOrder must be 1-8.");
        }

        ValidateText(profile.Slug, "names", profile.Names, errors);
        ValidateText(profile.Slug, "periods", profile.Periods, errors);
        ValidateText(profile.Slug, "diet", profile.Diet, errors);
        ValidateText(profile.Slug, "discoveryLocations", profile.DiscoveryLocations, errors);
        ValidateText(profile.Slug, "sizeDescription", profile.SizeDescription, errors);
        ValidateText(profile.Slug, "summary", profile.Summary, errors);
        ValidateReadableLimit(profile.Slug, "summary.zhTW", profile.Summary.ZhTW, LanguageCode.ZhTW, 1, 200, errors);
        ValidateReadableLimit(profile.Slug, "summary.en", profile.Summary.En, LanguageCode.En, 1, 200, errors);
        ValidateIllustration(profile.Slug, "mainImage", profile.MainImage, errors);
        ValidateStory(profile, errors);

        if (profile.Category == "prehistoric-flying-reptile")
        {
            if (profile.NotDinosaurNote is null)
            {
                errors.Add($"{profile.Slug}: notDinosaurNote is required for prehistoric-flying-reptile.");
            }
            else
            {
                ValidateText(profile.Slug, "notDinosaurNote", profile.NotDinosaurNote, errors);
            }
        }

        if (profile.SearchKeywords.Count == 0)
        {
            errors.Add($"{profile.Slug}: searchKeywords must not be empty.");
        }

        foreach (DinosaurText keyword in profile.SearchKeywords)
        {
            ValidateText(profile.Slug, "searchKeywords", keyword, errors);
        }
    }

    private static void ValidateStory(DinosaurProfile profile, List<string> errors)
    {
        ValidateText(profile.Slug, "story.title", profile.Story.Title, errors);
        ValidateText(profile.Slug, "story.body", profile.Story.Body, errors);
        ValidateReadableLimit(profile.Slug, "story.body.zhTW", profile.Story.Body.ZhTW, LanguageCode.ZhTW, 100, 150, errors);
        ValidateReadableLimit(profile.Slug, "story.body.en", profile.Story.Body.En, LanguageCode.En, 100, 150, errors);
        ValidateIllustration(profile.Slug, "story.illustration", profile.Story.Illustration, errors);
    }

    private static void ValidateIllustration(string slug, string field, DinosaurIllustration image, List<string> errors)
    {
        if (!ImagePathRegex().IsMatch(image.Path))
        {
            errors.Add($"{slug}: {field}.path must be a local dinosaur image path.");
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

    private static void ValidateText(string slug, string field, DinosaurText text, List<string> errors)
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

    private static void ValidateUniqueValues<T>(
        IReadOnlyList<DinosaurProfile> profiles,
        Func<DinosaurProfile, T> selector,
        string field,
        List<string> errors)
        where T : notnull
    {
        HashSet<T> values = [];

        foreach (DinosaurProfile profile in profiles)
        {
            if (!values.Add(selector(profile)))
            {
                errors.Add($"{profile.Slug}: duplicate {field}.");
            }
        }
    }

    [GeneratedRegex("^[a-z0-9]+(?:-[a-z0-9]+)*$")]
    private static partial Regex SlugRegex();

    [GeneratedRegex("^/images/dinosaurs/[a-z0-9-]+\\.(png|jpg|jpeg|webp)$", RegexOptions.IgnoreCase)]
    private static partial Regex ImagePathRegex();

    [GeneratedRegex("[\\p{L}\\p{N}]+")]
    private static partial Regex WordRegex();
}
