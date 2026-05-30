namespace StoryBook.Models;

/// <summary>
/// Availability and question count summary for one quiz source.
/// </summary>
public sealed class QuizSourceStatus
{
    /// <summary>
    /// Source storybook.
    /// </summary>
    public ExplorationSourceType Source { get; init; }

    /// <summary>
    /// Stable source code.
    /// </summary>
    public string SourceCode => Source.ToCode();

    /// <summary>
    /// Whether the source catalog could be loaded and used.
    /// </summary>
    public bool IsAvailable { get; init; }

    /// <summary>
    /// Number of stories available in this source.
    /// </summary>
    public int StoryCount { get; init; }

    /// <summary>
    /// Number of valid quiz questions for this source.
    /// </summary>
    public int QuestionCount { get; init; }

    /// <summary>
    /// Traditional Chinese friendly status message.
    /// </summary>
    public string? FriendlyMessageZhTW { get; init; }

    /// <summary>
    /// English friendly status message.
    /// </summary>
    public string? FriendlyMessageEn { get; init; }

    /// <summary>
    /// Non-sensitive diagnostic reason code.
    /// </summary>
    public string? ReasonCode { get; init; }

    /// <summary>
    /// Gets localized friendly message with Traditional Chinese fallback.
    /// </summary>
    public string GetFriendlyMessage(LanguageCode language)
    {
        return QuizQuestionView.GetLocalized(FriendlyMessageZhTW ?? string.Empty, FriendlyMessageEn ?? string.Empty, language);
    }
}
