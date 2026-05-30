namespace StoryBook.Models;

/// <summary>
/// Reference from a quiz question to an existing story detail page.
/// </summary>
public sealed class QuizStoryReference
{
    /// <summary>
    /// Referenced storybook source code.
    /// </summary>
    public string Source { get; init; } = string.Empty;

    /// <summary>
    /// Referenced story slug.
    /// </summary>
    public string Slug { get; init; } = string.Empty;

    /// <summary>
    /// Stable display order within the question.
    /// </summary>
    public int SortOrder { get; init; }
}
