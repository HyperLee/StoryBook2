namespace StoryBook.Models;

/// <summary>
/// Projection used by the list page for search result rendering and client-side filtering.
/// </summary>
public sealed class DinosaurSearchResult
{
    /// <summary>
    /// Profile slug used by the result link.
    /// </summary>
    public string Slug { get; init; } = string.Empty;

    /// <summary>
    /// Display name for the selected language.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Traditional Chinese display name.
    /// </summary>
    public string NameZhTW { get; init; } = string.Empty;

    /// <summary>
    /// English display name.
    /// </summary>
    public string NameEn { get; init; } = string.Empty;

    /// <summary>
    /// Short summary for the selected language.
    /// </summary>
    public string Summary { get; init; } = string.Empty;

    /// <summary>
    /// Traditional Chinese summary.
    /// </summary>
    public string SummaryZhTW { get; init; } = string.Empty;

    /// <summary>
    /// English summary.
    /// </summary>
    public string SummaryEn { get; init; } = string.Empty;

    /// <summary>
    /// Result image path.
    /// </summary>
    public string ImagePath { get; init; } = string.Empty;

    /// <summary>
    /// Result image alternative text.
    /// </summary>
    public string ImageAltText { get; init; } = string.Empty;

    /// <summary>
    /// Traditional Chinese image alternative text.
    /// </summary>
    public string ImageAltTextZhTW { get; init; } = string.Empty;

    /// <summary>
    /// English image alternative text.
    /// </summary>
    public string ImageAltTextEn { get; init; } = string.Empty;

    /// <summary>
    /// Text corpus used by browser-side filtering.
    /// </summary>
    public string SearchText { get; init; } = string.Empty;

    /// <summary>
    /// Optional category note, used for prehistoric flying reptiles.
    /// </summary>
    public string? CategoryNote { get; init; }

    /// <summary>
    /// Traditional Chinese category note.
    /// </summary>
    public string? CategoryNoteZhTW { get; init; }

    /// <summary>
    /// English category note.
    /// </summary>
    public string? CategoryNoteEn { get; init; }
}
