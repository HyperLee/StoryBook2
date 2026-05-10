using StoryBook.Models;

namespace StoryBook.Services;

/// <summary>
/// Resolves and exposes supported storybook language preference metadata.
/// </summary>
public sealed class LanguagePreferenceService
{
    /// <summary>
    /// Default language used when no valid preference exists.
    /// </summary>
    public LanguageCode DefaultLanguage => LanguageCode.ZhTW;

    /// <summary>
    /// Browser localStorage key for language preference.
    /// </summary>
    public string StorageKey => "storybook.language";

    /// <summary>
    /// Parses a stored language value, falling back to Traditional Chinese.
    /// </summary>
    public LanguageCode Parse(string? value)
    {
        return LanguageCodeParser.ParseOrDefault(value);
    }

    /// <summary>
    /// Converts a supported language to the persisted browser storage value.
    /// </summary>
    public string ToStorageValue(LanguageCode language)
    {
        return LanguageCodeParser.ToStorageValue(language);
    }
}
