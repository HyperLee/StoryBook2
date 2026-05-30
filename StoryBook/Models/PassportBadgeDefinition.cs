namespace StoryBook.Models;

/// <summary>
/// Fixed milestone metadata shown on the reading passport page.
/// </summary>
public sealed class PassportBadgeDefinition
{
    /// <summary>
    /// Stable badge code used by the browser to compute state.
    /// </summary>
    public string Code { get; init; } = string.Empty;

    /// <summary>
    /// Unlock rule type for this badge.
    /// </summary>
    public PassportBadgeMilestone Milestone { get; init; }

    /// <summary>
    /// Optional source code for source-scoped badges.
    /// </summary>
    public string? SourceCode { get; init; }

    /// <summary>
    /// Optional target count for count-based badges.
    /// </summary>
    public int? TargetCount { get; init; }

    /// <summary>
    /// Display order for the badge list.
    /// </summary>
    public int SortOrder { get; init; }

    /// <summary>
    /// Traditional Chinese badge label.
    /// </summary>
    public string LabelZhTW { get; init; } = string.Empty;

    /// <summary>
    /// English badge label.
    /// </summary>
    public string LabelEn { get; init; } = string.Empty;

    /// <summary>
    /// Traditional Chinese badge description.
    /// </summary>
    public string DescriptionZhTW { get; init; } = string.Empty;

    /// <summary>
    /// English badge description.
    /// </summary>
    public string DescriptionEn { get; init; } = string.Empty;

    /// <summary>
    /// Gets the localized label with a Traditional Chinese fallback.
    /// </summary>
    public string GetLabel(LanguageCode language)
    {
        return GetLocalizedValue(language, LabelZhTW, LabelEn);
    }

    /// <summary>
    /// Gets the localized description with a Traditional Chinese fallback.
    /// </summary>
    public string GetDescription(LanguageCode language)
    {
        return GetLocalizedValue(language, DescriptionZhTW, DescriptionEn);
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
