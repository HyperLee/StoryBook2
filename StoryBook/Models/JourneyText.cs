namespace StoryBook.Models;

/// <summary>
/// Stores bilingual learning journey text and resolves safe fallback content.
/// </summary>
public sealed class JourneyText
{
    /// <summary>
    /// Traditional Chinese text.
    /// </summary>
    public string ZhTW { get; init; } = string.Empty;

    /// <summary>
    /// English text. Missing or blank values fall back to <see cref="ZhTW" />.
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
