namespace StoryBook.Services;

/// <summary>
/// Configuration for the local quiz catalog file.
/// </summary>
public sealed class QuizCatalogOptions
{
    /// <summary>
    /// Configuration section name.
    /// </summary>
    public const string SectionName = "QuizCatalog";

    /// <summary>
    /// Path to the quiz catalog file, relative to content root unless rooted.
    /// </summary>
    public string ContentPath { get; init; } = "Data/quiz-questions.json";
}
