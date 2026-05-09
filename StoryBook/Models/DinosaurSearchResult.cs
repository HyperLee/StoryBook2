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
    /// Short summary for the selected language.
    /// </summary>
    public string Summary { get; init; } = string.Empty;

    /// <summary>
    /// Result image path.
    /// </summary>
    public string ImagePath { get; init; } = string.Empty;

    /// <summary>
    /// Result image alternative text.
    /// </summary>
    public string ImageAltText { get; init; } = string.Empty;

    /// <summary>
    /// Text corpus used by browser-side filtering.
    /// </summary>
    public string SearchText { get; init; } = string.Empty;

    /// <summary>
    /// Optional category note, used for prehistoric flying reptiles.
    /// </summary>
    public string? CategoryNote { get; init; }
}
