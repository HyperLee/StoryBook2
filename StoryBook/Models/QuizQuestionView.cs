namespace StoryBook.Models;

/// <summary>
/// Safe page projection for a quiz question. It never exposes the correct answer id.
/// </summary>
public sealed class QuizQuestionView
{
    /// <summary>
    /// Question id.
    /// </summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// Currently selected UI scope.
    /// </summary>
    public QuizScope Scope { get; init; }

    /// <summary>
    /// Stable scope code.
    /// </summary>
    public string ScopeCode => Scope.ToCode();

    /// <summary>
    /// Source storybook type.
    /// </summary>
    public ExplorationSourceType Source { get; init; }

    /// <summary>
    /// Stable source code.
    /// </summary>
    public string SourceCode => Source.ToCode();

    /// <summary>
    /// Traditional Chinese source label.
    /// </summary>
    public string SourceLabelZhTW { get; init; } = string.Empty;

    /// <summary>
    /// English source label.
    /// </summary>
    public string SourceLabelEn { get; init; } = string.Empty;

    /// <summary>
    /// Parsed difficulty value.
    /// </summary>
    public QuizDifficulty Difficulty { get; init; }

    /// <summary>
    /// Stable difficulty code.
    /// </summary>
    public string DifficultyCode => Difficulty.ToCode();

    /// <summary>
    /// Traditional Chinese prompt.
    /// </summary>
    public string PromptZhTW { get; init; } = string.Empty;

    /// <summary>
    /// English prompt.
    /// </summary>
    public string PromptEn { get; init; } = string.Empty;

    /// <summary>
    /// Safe answer option projections.
    /// </summary>
    public IReadOnlyList<QuizAnswerOptionView> Options { get; init; } = [];

    /// <summary>
    /// Existing story links for review.
    /// </summary>
    public IReadOnlyList<QuizRelatedStoryView> RelatedStories { get; init; } = [];

    /// <summary>
    /// Same-scope next question href.
    /// </summary>
    public string NextQuestionHref { get; init; } = string.Empty;

    /// <summary>
    /// Gets the localized source label with Traditional Chinese fallback.
    /// </summary>
    public string GetSourceLabel(LanguageCode language)
    {
        return GetLocalized(SourceLabelZhTW, SourceLabelEn, language);
    }

    /// <summary>
    /// Gets the localized prompt with Traditional Chinese fallback.
    /// </summary>
    public string GetPrompt(LanguageCode language)
    {
        return GetLocalized(PromptZhTW, PromptEn, language);
    }

    internal static string GetLocalized(string zhTW, string en, LanguageCode language)
    {
        string requested = language == LanguageCode.En ? en : zhTW;

        if (!string.IsNullOrWhiteSpace(requested))
        {
            return requested;
        }

        return !string.IsNullOrWhiteSpace(zhTW) ? zhTW : en;
    }
}

/// <summary>
/// Safe answer option projection for the quiz page.
/// </summary>
public sealed class QuizAnswerOptionView
{
    /// <summary>
    /// Option id.
    /// </summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// Stable display order.
    /// </summary>
    public int SortOrder { get; init; }

    /// <summary>
    /// Traditional Chinese option text.
    /// </summary>
    public string TextZhTW { get; init; } = string.Empty;

    /// <summary>
    /// English option text.
    /// </summary>
    public string TextEn { get; init; } = string.Empty;

    /// <summary>
    /// Gets localized option text with Traditional Chinese fallback.
    /// </summary>
    public string GetText(LanguageCode language)
    {
        return QuizQuestionView.GetLocalized(TextZhTW, TextEn, language);
    }
}

/// <summary>
/// Safe projection for a related story review link.
/// </summary>
public sealed class QuizRelatedStoryView
{
    /// <summary>
    /// Source storybook type.
    /// </summary>
    public ExplorationSourceType Source { get; init; }

    /// <summary>
    /// Stable source code.
    /// </summary>
    public string SourceCode => Source.ToCode();

    /// <summary>
    /// Story slug.
    /// </summary>
    public string Slug { get; init; } = string.Empty;

    /// <summary>
    /// Canonical story detail href.
    /// </summary>
    public string Href { get; init; } = string.Empty;

    /// <summary>
    /// Stable display order.
    /// </summary>
    public int SortOrder { get; init; }

    /// <summary>
    /// Traditional Chinese story label.
    /// </summary>
    public string LabelZhTW { get; init; } = string.Empty;

    /// <summary>
    /// English story label.
    /// </summary>
    public string LabelEn { get; init; } = string.Empty;

    /// <summary>
    /// Traditional Chinese source label.
    /// </summary>
    public string SourceLabelZhTW { get; init; } = string.Empty;

    /// <summary>
    /// English source label.
    /// </summary>
    public string SourceLabelEn { get; init; } = string.Empty;

    /// <summary>
    /// Gets localized story label with Traditional Chinese fallback.
    /// </summary>
    public string GetLabel(LanguageCode language)
    {
        return QuizQuestionView.GetLocalized(LabelZhTW, LabelEn, language);
    }

    /// <summary>
    /// Gets localized source label with Traditional Chinese fallback.
    /// </summary>
    public string GetSourceLabel(LanguageCode language)
    {
        return QuizQuestionView.GetLocalized(SourceLabelZhTW, SourceLabelEn, language);
    }
}
