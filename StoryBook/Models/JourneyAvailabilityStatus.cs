namespace StoryBook.Models;

/// <summary>
/// Availability states for a learning journey in the current catalog/source snapshot.
/// </summary>
public enum JourneyAvailabilityState
{
    /// <summary>
    /// The journey has all required text and 3-5 valid story items.
    /// </summary>
    Available,

    /// <summary>
    /// The requested journey slug does not exist.
    /// </summary>
    NotFound,

    /// <summary>
    /// The journey has fewer than 3 valid story items.
    /// </summary>
    NotEnoughItems,

    /// <summary>
    /// The journey has more than 5 valid story items.
    /// </summary>
    TooManyItems,

    /// <summary>
    /// Required journey text is missing after fallback.
    /// </summary>
    MissingRequiredText,

    /// <summary>
    /// A configured source storybook could not be used.
    /// </summary>
    SourceUnavailable,

    /// <summary>
    /// One or more story references are invalid.
    /// </summary>
    InvalidReference
}

/// <summary>
/// Describes whether one learning journey can appear in the list or detail page.
/// </summary>
public sealed class JourneyAvailabilityStatus
{
    /// <summary>
    /// Related journey slug.
    /// </summary>
    public string JourneySlug { get; init; } = string.Empty;

    /// <summary>
    /// Current journey availability state.
    /// </summary>
    public JourneyAvailabilityState State { get; init; }

    /// <summary>
    /// Count of valid story items after source resolution and duplicate filtering.
    /// </summary>
    public int ValidItemCount { get; init; }

    /// <summary>
    /// Whether this journey may appear as a selectable card on <c>/journeys</c>.
    /// </summary>
    public bool CanAppearInList { get; init; }

    /// <summary>
    /// Bilingual child-friendly message for unavailable states.
    /// </summary>
    public JourneyText FriendlyMessage { get; init; } = new();

    /// <summary>
    /// Non-sensitive diagnostic summary for logs and tests.
    /// </summary>
    public JourneyDiagnosticSummary? DiagnosticSummary { get; init; }

    /// <summary>
    /// Gets the localized friendly message with a Traditional Chinese fallback.
    /// </summary>
    public string GetFriendlyMessage(LanguageCode language)
    {
        return FriendlyMessage.Get(language);
    }
}
