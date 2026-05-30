namespace StoryBook.Models;

/// <summary>
/// Projection for one existing story that can appear in the reading passport.
/// </summary>
public sealed class PassportStoryItem
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
    /// Lowercase source code stored in the browser passport state.
    /// </summary>
    public string SourceCode { get; init; } = string.Empty;

    /// <summary>
    /// Source catalog slug.
    /// </summary>
    public string Slug { get; init; } = string.Empty;

    /// <summary>
    /// Sort order within the source catalog.
    /// </summary>
    public int SortOrder { get; init; }

    /// <summary>
    /// Sort order of the source storybook.
    /// </summary>
    public int SourceSortOrder { get; init; }

    /// <summary>
    /// Canonical existing detail page href.
    /// </summary>
    public string DetailHref { get; init; } = string.Empty;

    /// <summary>
    /// Traditional Chinese source label.
    /// </summary>
    public string SourceLabelZhTW { get; init; } = string.Empty;

    /// <summary>
    /// English source label.
    /// </summary>
    public string SourceLabelEn { get; init; } = string.Empty;

    /// <summary>
    /// Traditional Chinese story friend name.
    /// </summary>
    public string NameZhTW { get; init; } = string.Empty;

    /// <summary>
    /// English story friend name.
    /// </summary>
    public string NameEn { get; init; } = string.Empty;

    /// <summary>
    /// Traditional Chinese story summary.
    /// </summary>
    public string SummaryZhTW { get; init; } = string.Empty;

    /// <summary>
    /// English story summary.
    /// </summary>
    public string SummaryEn { get; init; } = string.Empty;

    /// <summary>
    /// Optional main image path.
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
    /// Gets the localized source label with a Traditional Chinese fallback.
    /// </summary>
    public string GetSourceLabel(LanguageCode language)
    {
        return GetLocalizedValue(language, SourceLabelZhTW, SourceLabelEn);
    }

    /// <summary>
    /// Gets the localized story friend name with a Traditional Chinese fallback.
    /// </summary>
    public string GetName(LanguageCode language)
    {
        return GetLocalizedValue(language, NameZhTW, NameEn);
    }

    /// <summary>
    /// Gets the localized summary with a Traditional Chinese fallback.
    /// </summary>
    public string GetSummary(LanguageCode language)
    {
        return GetLocalizedValue(language, SummaryZhTW, SummaryEn);
    }

    /// <summary>
    /// Gets the localized image alternative text with a Traditional Chinese fallback.
    /// </summary>
    public string GetImageAltText(LanguageCode language)
    {
        return GetLocalizedValue(language, ImageAltTextZhTW ?? string.Empty, ImageAltTextEn ?? string.Empty);
    }

    private static string GetLocalizedValue(LanguageCode language, string zhTW, string en)
    {
        string requested = language == LanguageCode.En ? en : zhTW;

        if (!string.IsNullOrWhiteSpace(requested))
        {
            return requested;
        }

        return !string.IsNullOrWhiteSpace(zhTW) ? zhTW : en;
    }
}
