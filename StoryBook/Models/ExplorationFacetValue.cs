namespace StoryBook.Models;

/// <summary>
/// Represents one selectable exploration facet value.
/// </summary>
public sealed class ExplorationFacetValue
{
    /// <summary>
    /// Facet group code, such as <c>source</c>, <c>diet</c>, or <c>living-area</c>.
    /// </summary>
    public string GroupCode { get; init; } = string.Empty;

    /// <summary>
    /// Stable value code suitable for HTML data attributes.
    /// </summary>
    public string ValueCode { get; init; } = string.Empty;

    /// <summary>
    /// Traditional Chinese label.
    /// </summary>
    public string LabelZhTW { get; init; } = string.Empty;

    /// <summary>
    /// English label.
    /// </summary>
    public string LabelEn { get; init; } = string.Empty;

    /// <summary>
    /// Optional source scope when the value applies only to one storybook.
    /// </summary>
    public ExplorationSourceType? SourceScope { get; init; }

    /// <summary>
    /// Stable display order inside the group.
    /// </summary>
    public int SortOrder { get; init; }

    /// <summary>
    /// Gets the localized label with a Traditional Chinese fallback.
    /// </summary>
    public string GetLabel(LanguageCode language)
    {
        string requested = language == LanguageCode.En ? LabelEn : LabelZhTW;

        if (!string.IsNullOrWhiteSpace(requested))
        {
            return requested;
        }

        return !string.IsNullOrWhiteSpace(LabelZhTW) ? LabelZhTW : LabelEn;
    }
}
