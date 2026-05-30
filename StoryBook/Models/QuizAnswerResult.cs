namespace StoryBook.Models;

/// <summary>
/// Transient result for one answer submission. This type is not persisted.
/// </summary>
public sealed class QuizAnswerResult
{
    /// <summary>
    /// Question id that was answered.
    /// </summary>
    public string QuestionId { get; init; } = string.Empty;

    /// <summary>
    /// Selected option id, or null when no valid option was selected.
    /// </summary>
    public string? SelectedOptionId { get; init; }

    /// <summary>
    /// Whether the user submitted a valid option.
    /// </summary>
    public bool IsAnswered { get; init; }

    /// <summary>
    /// Correctness for a valid answer, or null for no-selection/unknown-option prompts.
    /// </summary>
    public bool? IsCorrect { get; init; }

    /// <summary>
    /// Traditional Chinese feedback.
    /// </summary>
    public string FeedbackZhTW { get; init; } = string.Empty;

    /// <summary>
    /// English feedback.
    /// </summary>
    public string FeedbackEn { get; init; } = string.Empty;

    /// <summary>
    /// Traditional Chinese explanation for a valid answer.
    /// </summary>
    public string? ExplanationZhTW { get; init; }

    /// <summary>
    /// English explanation for a valid answer.
    /// </summary>
    public string? ExplanationEn { get; init; }

    /// <summary>
    /// Gets localized feedback with Traditional Chinese fallback.
    /// </summary>
    public string GetFeedback(LanguageCode language)
    {
        return QuizQuestionView.GetLocalized(FeedbackZhTW, FeedbackEn, language);
    }

    /// <summary>
    /// Gets localized explanation with Traditional Chinese fallback.
    /// </summary>
    public string? GetExplanation(LanguageCode language)
    {
        string explanation = QuizQuestionView.GetLocalized(ExplanationZhTW ?? string.Empty, ExplanationEn ?? string.Empty, language);
        return string.IsNullOrWhiteSpace(explanation) ? null : explanation;
    }
}
