namespace StoryBook.Models;

/// <summary>
/// A single reviewed multiple-choice quiz question from the local catalog.
/// </summary>
public sealed class QuizQuestion
{
    /// <summary>
    /// Unique stable kebab-case question id.
    /// </summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// Source storybook code. Only dinosaurs and aquarium are valid catalog values.
    /// </summary>
    public string Source { get; init; } = string.Empty;

    /// <summary>
    /// Difficulty code. Only easy and medium are valid catalog values.
    /// </summary>
    public string Difficulty { get; init; } = string.Empty;

    /// <summary>
    /// Stable order within the source.
    /// </summary>
    public int SortOrder { get; init; }

    /// <summary>
    /// Bilingual question prompt.
    /// </summary>
    public QuizText Prompt { get; init; } = new();

    /// <summary>
    /// Candidate answer options.
    /// </summary>
    public IReadOnlyList<QuizAnswerOption> Options { get; init; } = [];

    /// <summary>
    /// Id of the unique correct option.
    /// </summary>
    public string CorrectOptionId { get; init; } = string.Empty;

    /// <summary>
    /// Bilingual feedback shown for a correct answer.
    /// </summary>
    public QuizText CorrectFeedback { get; init; } = new();

    /// <summary>
    /// Bilingual feedback shown for an incorrect answer.
    /// </summary>
    public QuizText IncorrectFeedback { get; init; } = new();

    /// <summary>
    /// Bilingual explanation shown after a valid answer.
    /// </summary>
    public QuizText Explanation { get; init; } = new();

    /// <summary>
    /// References to existing stories for review.
    /// </summary>
    public IReadOnlyList<QuizStoryReference> RelatedStories { get; init; } = [];
}
