namespace StoryBook.Models;

/// <summary>
/// Describes whether one storybook source contributed stories to the reading passport.
/// </summary>
public sealed class PassportSourceStatus
{
    /// <summary>
    /// Source storybook.
    /// </summary>
    public ExplorationSourceType Source { get; init; }

    /// <summary>
    /// Lowercase source code used by the browser passport state.
    /// </summary>
    public string SourceCode { get; init; } = string.Empty;

    /// <summary>
    /// Whether the source successfully provided stories.
    /// </summary>
    public bool IsAvailable { get; init; }

    /// <summary>
    /// Count of available stories from this source.
    /// </summary>
    public int StoryCount { get; init; }

    /// <summary>
    /// Traditional Chinese friendly message for unavailable sources.
    /// </summary>
    public string? FriendlyMessageZhTW { get; init; }

    /// <summary>
    /// English friendly message for unavailable sources.
    /// </summary>
    public string? FriendlyMessageEn { get; init; }

    /// <summary>
    /// Non-sensitive reason code for diagnostics.
    /// </summary>
    public string? ReasonCode { get; init; }

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
        return GetLocalizedValue(language, FriendlyMessageZhTW ?? string.Empty, FriendlyMessageEn ?? string.Empty);
    }

    private static string GetLocalizedValue(LanguageCode language, string zhTW, string en)
    {
        string requested = language == LanguageCode.En ? en : zhTW;

        if (!string.IsNullOrWhiteSpace(requested))
        {
            return requested;
        }

        return !string.IsNullOrWhiteSpace(zhTW) ? zhTW : en;
    }
}
