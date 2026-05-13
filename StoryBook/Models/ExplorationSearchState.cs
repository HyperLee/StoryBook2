namespace StoryBook.Models;

/// <summary>
/// Describes the result mode currently shown by the exploration page.
/// </summary>
public enum ExplorationResultMode
{
    /// <summary>
    /// The complete available collection is visible.
    /// </summary>
    All,

    /// <summary>
    /// A valid search query is active.
    /// </summary>
    Search,

    /// <summary>
    /// One or more facet filters are active.
    /// </summary>
    Filter,

    /// <summary>
    /// A valid search query and facet filters are both active.
    /// </summary>
    Intersection,

    /// <summary>
    /// The raw query has content, but normalization leaves fewer than two searchable characters.
    /// </summary>
    TooShort,

    /// <summary>
    /// A valid search or filter state has no matching results.
    /// </summary>
    NoResults,

    /// <summary>
    /// At least one source failed while another source remained available.
    /// </summary>
    PartialFailure,

    /// <summary>
    /// All sources failed.
    /// </summary>
    AllFailed
}

/// <summary>
/// Stores page-local exploration search and filter state.
/// </summary>
public sealed class ExplorationSearchState
{
    /// <summary>
    /// Raw query text from the current page input.
    /// </summary>
    public string RawQuery { get; init; } = string.Empty;

    /// <summary>
    /// Normalized query used for matching.
    /// </summary>
    public string NormalizedQuery { get; init; } = string.Empty;

    /// <summary>
    /// Selected facet values keyed by group code.
    /// </summary>
    public IReadOnlyDictionary<string, string> SelectedFacetValues { get; init; } = new Dictionary<string, string>();

    /// <summary>
    /// Current display language.
    /// </summary>
    public LanguageCode Language { get; init; } = LanguageCode.ZhTW;

    /// <summary>
    /// Current result mode.
    /// </summary>
    public ExplorationResultMode ResultMode { get; init; } = ExplorationResultMode.All;

    /// <summary>
    /// Count of visible result cards.
    /// </summary>
    public int VisibleResultCount { get; init; }
}

/// <summary>
/// Search service result containing matched items and the page-local state metadata.
/// </summary>
public sealed class ExplorationSearchResult
{
    /// <summary>
    /// Items matching the current search and filter state.
    /// </summary>
    public IReadOnlyList<ExplorationItem> Items { get; init; } = [];

    /// <summary>
    /// State metadata for the current result set.
    /// </summary>
    public ExplorationSearchState State { get; init; } = new();
}
