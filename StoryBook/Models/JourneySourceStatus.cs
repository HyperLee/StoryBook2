namespace StoryBook.Models;

/// <summary>
/// Describes whether one source storybook contributed items to learning journeys.
/// </summary>
public sealed class JourneySourceStatus
{
    /// <summary>
    /// Source storybook.
    /// </summary>
    public ExplorationSourceType Source { get; init; }

    /// <summary>
    /// Whether the source successfully provided story items.
    /// </summary>
    public bool IsAvailable { get; init; }

    /// <summary>
    /// Count of source items available for lookup.
    /// </summary>
    public int ItemCount { get; init; }

    /// <summary>
    /// Bilingual child-friendly message for unavailable source states.
    /// </summary>
    public JourneyText? FriendlyMessage { get; init; }

    /// <summary>
    /// Non-sensitive diagnostic summary for logs and tests.
    /// </summary>
    public JourneyDiagnosticSummary? DiagnosticSummary { get; init; }

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
        return FriendlyMessage?.Get(language) ?? string.Empty;
    }
}
