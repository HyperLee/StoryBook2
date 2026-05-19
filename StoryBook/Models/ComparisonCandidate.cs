namespace StoryBook.Models;

/// <summary>
/// Projection for one story friend that can be selected on the comparison page.
/// </summary>
public sealed class ComparisonCandidate
{
    /// <summary>
    /// Stable id in the format <c>{source}:{slug}</c>.
    /// </summary>
    public string StableId { get; init; } = string.Empty;

    /// <summary>
    /// Source storybook.
    /// </summary>
    public ExplorationSourceType Source { get; init; }

    /// <summary>
    /// Source catalog slug.
    /// </summary>
    public string Slug { get; init; } = string.Empty;

    /// <summary>
    /// Canonical existing detail page href.
    /// </summary>
    public string DetailHref { get; init; } = string.Empty;

    /// <summary>
    /// Sort order within the source catalog.
    /// </summary>
    public int SortOrder { get; init; }

    /// <summary>
    /// Sort order of the source storybook.
    /// </summary>
    public int SourceSortOrder { get; init; }

    /// <summary>
    /// Traditional Chinese source label.
    /// </summary>
    public string SourceLabelZhTW { get; init; } = string.Empty;

    /// <summary>
    /// English source label.
    /// </summary>
    public string SourceLabelEn { get; init; } = string.Empty;

    /// <summary>
    /// Traditional Chinese display name.
    /// </summary>
    public string NameZhTW { get; init; } = string.Empty;

    /// <summary>
    /// English display name.
    /// </summary>
    public string NameEn { get; init; } = string.Empty;

    /// <summary>
    /// Traditional Chinese summary.
    /// </summary>
    public string SummaryZhTW { get; init; } = string.Empty;

    /// <summary>
    /// English summary.
    /// </summary>
    public string SummaryEn { get; init; } = string.Empty;

    /// <summary>
    /// Traditional Chinese diet text.
    /// </summary>
    public string DietZhTW { get; init; } = string.Empty;

    /// <summary>
    /// English diet text.
    /// </summary>
    public string DietEn { get; init; } = string.Empty;

    /// <summary>
    /// Traditional Chinese discovery location or common distribution text.
    /// </summary>
    public string DiscoveryLocationsZhTW { get; init; } = string.Empty;

    /// <summary>
    /// English discovery location or common distribution text.
    /// </summary>
    public string DiscoveryLocationsEn { get; init; } = string.Empty;

    /// <summary>
    /// Optional main image path for future compare visuals.
    /// </summary>
    public string? ImagePath { get; init; }

    /// <summary>
    /// Optional Traditional Chinese image alternative text.
    /// </summary>
    public string? ImageAltTextZhTW { get; init; }

    /// <summary>
    /// Optional English image alternative text.
    /// </summary>
    public string? ImageAltTextEn { get; init; }

    /// <summary>
    /// Field values keyed by comparison field code.
    /// </summary>
    public IReadOnlyDictionary<string, ComparisonFieldValue> FieldValues { get; init; } =
        new Dictionary<string, ComparisonFieldValue>(StringComparer.Ordinal);

    /// <summary>
    /// Gets the localized source label with a Traditional Chinese fallback.
    /// </summary>
    public string GetSourceLabel(LanguageCode language)
    {
        return GetLocalized(SourceLabelZhTW, SourceLabelEn, language);
    }

    /// <summary>
    /// Gets the localized display name with a Traditional Chinese fallback.
    /// </summary>
    public string GetName(LanguageCode language)
    {
        return GetLocalized(NameZhTW, NameEn, language);
    }

    /// <summary>
    /// Gets the localized summary with a Traditional Chinese fallback.
    /// </summary>
    public string GetSummary(LanguageCode language)
    {
        return GetLocalized(SummaryZhTW, SummaryEn, language);
    }

    /// <summary>
    /// Gets the localized diet text with a Traditional Chinese fallback.
    /// </summary>
    public string GetDiet(LanguageCode language)
    {
        return GetLocalized(DietZhTW, DietEn, language);
    }

    /// <summary>
    /// Gets the localized discovery location text with a Traditional Chinese fallback.
    /// </summary>
    public string GetDiscoveryLocations(LanguageCode language)
    {
        return GetLocalized(DiscoveryLocationsZhTW, DiscoveryLocationsEn, language);
    }

    /// <summary>
    /// Gets the localized image alternative text with a Traditional Chinese fallback.
    /// </summary>
    public string GetImageAltText(LanguageCode language)
    {
        return GetLocalized(ImageAltTextZhTW ?? string.Empty, ImageAltTextEn ?? string.Empty, language);
    }

    /// <summary>
    /// Gets a field value by code, returning an empty available value if the field is missing.
    /// </summary>
    public ComparisonFieldValue GetFieldValue(string code)
    {
        return FieldValues.TryGetValue(code, out ComparisonFieldValue? value)
            ? value
            : new ComparisonFieldValue();
    }

    private static string GetLocalized(string zhTW, string en, LanguageCode language)
    {
        string requested = language == LanguageCode.En ? en : zhTW;

        if (!string.IsNullOrWhiteSpace(requested))
        {
            return requested;
        }

        return !string.IsNullOrWhiteSpace(zhTW) ? zhTW : en;
    }
}
