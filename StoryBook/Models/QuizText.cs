namespace StoryBook.Models;

/// <summary>
/// Stores bilingual quiz text and resolves a safe Traditional Chinese fallback.
/// </summary>
public sealed class QuizText
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
            return requested.Trim();
        }

        if (!string.IsNullOrWhiteSpace(ZhTW))
        {
            return ZhTW.Trim();
        }

        return En.Trim();
    }

    /// <summary>
    /// Gets text for a persisted language value, falling back to Traditional Chinese.
    /// </summary>
    public string Get(string? language)
    {
        return Get(LanguageCodeParser.ParseOrDefault(language));
    }
}
