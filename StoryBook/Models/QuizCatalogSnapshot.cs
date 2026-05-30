namespace StoryBook.Models;

/// <summary>
/// Current quiz catalog state after loading, validation, and story reference resolution.
/// </summary>
public sealed class QuizCatalogSnapshot
{
    /// <summary>
    /// Creates a snapshot from valid questions, source statuses, and invalid question count.
    /// </summary>
    public QuizCatalogSnapshot(
        IReadOnlyList<QuizQuestion> questions,
        IReadOnlyList<QuizSourceStatus> sourceStatuses,
        int invalidQuestionCount)
    {
        Questions = questions;
        SourceStatuses = sourceStatuses;
        InvalidQuestionCount = invalidQuestionCount;
    }

    /// <summary>
    /// Valid questions that are safe to project.
    /// </summary>
    public IReadOnlyList<QuizQuestion> Questions { get; }

    /// <summary>
    /// Source availability summaries.
    /// </summary>
    public IReadOnlyList<QuizSourceStatus> SourceStatuses { get; }

    /// <summary>
    /// Number of invalid questions filtered from the catalog.
    /// </summary>
    public int InvalidQuestionCount { get; }

    /// <summary>
    /// Whether at least one question is available.
    /// </summary>
    public bool HasAnyQuestion => Questions.Count > 0;

    /// <summary>
    /// Whether one source failed while at least one source remains available.
    /// </summary>
    public bool HasPartialSourceFailure => SourceStatuses.Any(status => !status.IsAvailable)
        && SourceStatuses.Any(status => status.IsAvailable);

    /// <summary>
    /// Whether all source catalogs are unavailable.
    /// </summary>
    public bool HasAllSourcesFailed => SourceStatuses.Count > 0
        && SourceStatuses.All(status => !status.IsAvailable);
}
