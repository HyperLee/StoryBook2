namespace StoryBook.Models;

/// <summary>
/// Projection for one story item that can be displayed, searched, and filtered on <c>/explore</c>.
/// </summary>
public sealed class ExplorationItem
{
    /// <summary>
    /// Globally unique id in the format <c>{source}:{slug}</c>.
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
    /// Canonical detail page href.
    /// </summary>
    public string DetailHref { get; init; } = string.Empty;

    /// <summary>
    /// Sort order within the source catalog.
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
    /// Traditional Chinese display name.
    /// </summary>
    public string NameZhTW { get; init; } = string.Empty;

    /// <summary>
    /// English display name.
    /// </summary>
    public string NameEn { get; init; } = string.Empty;

    /// <summary>
    /// Traditional Chinese short summary.
    /// </summary>
    public string SummaryZhTW { get; init; } = string.Empty;

    /// <summary>
    /// English short summary.
    /// </summary>
    public string SummaryEn { get; init; } = string.Empty;

    /// <summary>
    /// Main image path.
    /// </summary>
    public string ImagePath { get; init; } = string.Empty;

    /// <summary>
    /// Traditional Chinese image alternative text.
    /// </summary>
    public string ImageAltTextZhTW { get; init; } = string.Empty;

    /// <summary>
    /// English image alternative text.
    /// </summary>
    public string ImageAltTextEn { get; init; } = string.Empty;

    /// <summary>
    /// Bilingual search index text emitted for page-local filtering.
    /// </summary>
    public string SearchText { get; init; } = string.Empty;

    /// <summary>
    /// Facets that this item matches.
    /// </summary>
    public IReadOnlyList<ExplorationFacetValue> Facets { get; init; } = [];

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
    /// Gets the localized image alternative text with a Traditional Chinese fallback.
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
