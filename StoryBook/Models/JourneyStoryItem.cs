namespace StoryBook.Models;

/// <summary>
/// Display projection for one resolved story item inside a learning journey.
/// </summary>
public sealed class JourneyStoryItem
{
    /// <summary>
    /// Stable item id in the form <c>{source}:{slug}</c>.
    /// </summary>
    public string StableId { get; init; } = string.Empty;

    /// <summary>
    /// Source storybook.
    /// </summary>
    public ExplorationSourceType Source { get; init; }

    /// <summary>
    /// Source story slug.
    /// </summary>
    public string Slug { get; init; } = string.Empty;

    /// <summary>
    /// Normal anchor href to the source story detail page.
    /// </summary>
    public string DetailHref { get; init; } = string.Empty;

    /// <summary>
    /// Reading order inside the journey.
    /// </summary>
    public int SortOrder { get; init; }

    /// <summary>
    /// Traditional Chinese source label.
    /// </summary>
    public string SourceLabelZhTW { get; init; } = string.Empty;

    /// <summary>
    /// English source label.
    /// </summary>
    public string SourceLabelEn { get; init; } = string.Empty;

    /// <summary>
    /// Traditional Chinese source story name.
    /// </summary>
    public string NameZhTW { get; init; } = string.Empty;

    /// <summary>
    /// English source story name.
    /// </summary>
    public string NameEn { get; init; } = string.Empty;

    /// <summary>
    /// Traditional Chinese source story summary.
    /// </summary>
    public string SummaryZhTW { get; init; } = string.Empty;

    /// <summary>
    /// English source story summary.
    /// </summary>
    public string SummaryEn { get; init; } = string.Empty;

    /// <summary>
    /// Optional image path from the source story.
    /// </summary>
    public string? ImagePath { get; init; }

    /// <summary>
    /// Traditional Chinese image alternate text.
    /// </summary>
    public string ImageAltTextZhTW { get; init; } = string.Empty;

    /// <summary>
    /// English image alternate text.
    /// </summary>
    public string ImageAltTextEn { get; init; } = string.Empty;

    /// <summary>
    /// Gets the localized source label with a Traditional Chinese fallback.
    /// </summary>
    public string GetSourceLabel(LanguageCode language)
    {
        return GetLocalized(SourceLabelZhTW, SourceLabelEn, language);
    }

    /// <summary>
    /// Gets the localized source story name with a Traditional Chinese fallback.
    /// </summary>
    public string GetName(LanguageCode language)
    {
        return GetLocalized(NameZhTW, NameEn, language);
    }

    /// <summary>
    /// Gets the localized source story summary with a Traditional Chinese fallback.
    /// </summary>
    public string GetSummary(LanguageCode language)
    {
        return GetLocalized(SummaryZhTW, SummaryEn, language);
    }

    /// <summary>
    /// Gets the localized image alternate text with a Traditional Chinese fallback.
    /// </summary>
    public string GetImageAltText(LanguageCode language)
    {
        return GetLocalized(ImageAltTextZhTW, ImageAltTextEn, language);
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
