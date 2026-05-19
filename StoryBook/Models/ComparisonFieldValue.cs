namespace StoryBook.Models;

/// <summary>
/// Describes whether a comparison field has a real value or a child-friendly not-applicable value.
/// </summary>
public enum ComparisonFieldValueState
{
    /// <summary>
    /// The field has a value from the source storybook content.
    /// </summary>
    Available,

    /// <summary>
    /// The field does not apply to this story friend.
    /// </summary>
    NotApplicable
}

/// <summary>
/// Localized value shown for one candidate in one comparison field.
/// </summary>
public sealed class ComparisonFieldValue
{
    /// <summary>
    /// Whether this value is available or represents a not-applicable state.
    /// </summary>
    public ComparisonFieldValueState State { get; init; }

    /// <summary>
    /// Traditional Chinese display text.
    /// </summary>
    public string TextZhTW { get; init; } = string.Empty;

    /// <summary>
    /// English display text.
    /// </summary>
    public string TextEn { get; init; } = string.Empty;

    /// <summary>
    /// Optional detail href for link-style fields.
    /// </summary>
    public string? Href { get; init; }

    /// <summary>
    /// Gets localized display text with a Traditional Chinese fallback.
    /// </summary>
    public string GetText(LanguageCode language)
    {
        return GetLocalized(TextZhTW, TextEn, language);
    }

    private static string GetLocalized(string zhTW, string en, LanguageCode language)
    {
        string requested = language == LanguageCode.En ? en : zhTW;

        if (!string.IsNullOrWhiteSpace(requested))
        {
            return requested;
        }

        return !string.IsNullOrWhiteSpace(zhTW) ? zhTW : en;
    }
}
