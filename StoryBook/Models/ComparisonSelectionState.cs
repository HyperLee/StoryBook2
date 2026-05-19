namespace StoryBook.Models;

/// <summary>
/// Documents the page-local selection state managed by compare JavaScript.
/// </summary>
public sealed class ComparisonSelectionState
{
    /// <summary>
    /// First selected candidate stable id for the current page lifetime.
    /// </summary>
    public string? FirstCandidateId { get; init; }

    /// <summary>
    /// Second selected candidate stable id for the current page lifetime.
    /// </summary>
    public string? SecondCandidateId { get; init; }

    /// <summary>
    /// Current page-local status such as empty, one-selected, duplicate, ready, or not-enough-candidates.
    /// </summary>
    public string Status { get; init; } = "empty";

    /// <summary>
    /// Current display language, falling back to Traditional Chinese when invalid.
    /// </summary>
    public LanguageCode Language { get; init; } = LanguageCode.ZhTW;
}
