namespace StoryBook.Models;

/// <summary>
/// Describes whether one storybook source contributed candidates to the comparison page.
/// </summary>
public sealed class ComparisonSourceStatus
{
    /// <summary>
    /// Source storybook.
    /// </summary>
    public ExplorationSourceType Source { get; init; }

    /// <summary>
    /// Whether the source successfully provided candidates.
    /// </summary>
    public bool IsAvailable { get; init; }

    /// <summary>
    /// Count of available candidates from this source.
    /// </summary>
    public int CandidateCount { get; init; }

    /// <summary>
    /// Traditional Chinese friendly message for unavailable sources.
    /// </summary>
    public string FriendlyMessageZhTW { get; init; } = string.Empty;

    /// <summary>
    /// English friendly message for unavailable sources.
    /// </summary>
    public string FriendlyMessageEn { get; init; } = string.Empty;

    /// <summary>
    /// Gets the localized source label with a Traditional Chinese fallback.
    /// </summary>
    public string GetSourceLabel(LanguageCode language)
    {
        return Source.GetLabel(language);
    }

    /// <summary>
    /// Gets the localized friendly message with a Traditional Chinese fallback.
    /// </summary>
    public string GetFriendlyMessage(LanguageCode language)
    {
        string requested = language == LanguageCode.En ? FriendlyMessageEn : FriendlyMessageZhTW;

        if (!string.IsNullOrWhiteSpace(requested))
        {
            return requested;
        }

        return !string.IsNullOrWhiteSpace(FriendlyMessageZhTW) ? FriendlyMessageZhTW : FriendlyMessageEn;
    }
}
