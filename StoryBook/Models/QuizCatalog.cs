namespace StoryBook.Models;

/// <summary>
/// Root object for the local quiz questions JSON catalog.
/// </summary>
public sealed class QuizCatalog
{
    /// <summary>
    /// Catalog schema version. MVP supports version 1.
    /// </summary>
    public int Version { get; init; }

    /// <summary>
    /// Reviewed quiz questions.
    /// </summary>
    public IReadOnlyList<QuizQuestion> Questions { get; init; } = [];
}
