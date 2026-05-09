namespace StoryBook.Models;

/// <summary>
/// Stores bilingual text and resolves a safe fallback when a localized value is missing.
/// </summary>
public sealed class DinosaurText
{
    /// <summary>
    /// Traditional Chinese text.
    /// </summary>
    public string ZhTW { get; init; } = string.Empty;

    /// <summary>
    /// English text.
    /// </summary>
    public string En { get; init; } = string.Empty;

    /// <summary>
    /// Gets text for the requested language, falling back to Traditional Chinese and then English.
    /// </summary>
    public string Get(LanguageCode language)
    {
        string requested = language == LanguageCode.En ? En : ZhTW;

        if (!string.IsNullOrWhiteSpace(requested))
        {
            return requested;
        }

        if (!string.IsNullOrWhiteSpace(ZhTW))
        {
            return ZhTW;
        }

        return En;
    }

    /// <summary>
    /// Gets text for a storage or route language value, falling back to Traditional Chinese.
    /// </summary>
    public string Get(string? language)
    {
        return Get(LanguageCodeParser.ParseOrDefault(language));
    }
}
