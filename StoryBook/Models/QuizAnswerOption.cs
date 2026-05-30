namespace StoryBook.Models;

/// <summary>
/// A single answer option in a quiz question.
/// </summary>
public sealed class QuizAnswerOption
{
    /// <summary>
    /// Stable option id unique within its question.
    /// </summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// Bilingual option text.
    /// </summary>
    public QuizText Text { get; init; } = new();

    /// <summary>
    /// Stable display order.
    /// </summary>
    public int SortOrder { get; init; }
}
