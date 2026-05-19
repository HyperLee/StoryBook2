namespace StoryBook.Models;

/// <summary>
/// Defines one row in the story friend comparison table.
/// </summary>
public sealed class ComparisonFieldDefinition
{
    /// <summary>
    /// Stable row code emitted in data attributes.
    /// </summary>
    public string Code { get; init; } = string.Empty;

    /// <summary>
    /// Traditional Chinese row label.
    /// </summary>
    public string LabelZhTW { get; init; } = string.Empty;

    /// <summary>
    /// English row label.
    /// </summary>
    public string LabelEn { get; init; } = string.Empty;

    /// <summary>
    /// Display order in the comparison table.
    /// </summary>
    public int SortOrder { get; init; }

    /// <summary>
    /// Traditional Chinese child-friendly text used when this field does not apply.
    /// </summary>
    public string NotApplicableTextZhTW { get; init; } = string.Empty;

    /// <summary>
    /// English child-friendly text used when this field does not apply.
    /// </summary>
    public string NotApplicableTextEn { get; init; } = string.Empty;

    /// <summary>
    /// Gets the localized label with a Traditional Chinese fallback.
    /// </summary>
    public string GetLabel(LanguageCode language)
    {
        return GetLocalized(LabelZhTW, LabelEn, language);
    }

    /// <summary>
    /// Gets the localized not-applicable text with a Traditional Chinese fallback.
    /// </summary>
    public string GetNotApplicableText(LanguageCode language)
    {
        return GetLocalized(NotApplicableTextZhTW, NotApplicableTextEn, language);
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
