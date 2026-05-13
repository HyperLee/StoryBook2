namespace StoryBook.Models;

/// <summary>
/// Represents a single-selection exploration filter group.
/// </summary>
public sealed class ExplorationFacetGroup
{
    /// <summary>
    /// Stable group code used by markup and scripts.
    /// </summary>
    public string Code { get; init; } = string.Empty;

    /// <summary>
    /// Traditional Chinese group label.
    /// </summary>
    public string LabelZhTW { get; init; } = string.Empty;

    /// <summary>
    /// English group label.
    /// </summary>
    public string LabelEn { get; init; } = string.Empty;

    /// <summary>
    /// Stable display order for the group.
    /// </summary>
    public int SortOrder { get; init; }

    /// <summary>
    /// Selection mode. Exploration facets are single-select per group.
    /// </summary>
    public string SelectionMode { get; init; } = "single";

    /// <summary>
    /// Available values for the group.
    /// </summary>
    public IReadOnlyList<ExplorationFacetValue> Values { get; init; } = [];

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
