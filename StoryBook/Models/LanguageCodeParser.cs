namespace StoryBook.Models;

/// <summary>
/// Converts persisted language values into supported language codes.
/// </summary>
public static class LanguageCodeParser
{
    /// <summary>
    /// Converts a supported language code to the localStorage value used by the frontend.
    /// </summary>
    public static string ToStorageValue(LanguageCode language)
    {
        return language == LanguageCode.En ? "en" : "zh-TW";
    }

    /// <summary>
    /// Parses a language value, returning Traditional Chinese for null, empty, or unsupported values.
    /// </summary>
    public static LanguageCode ParseOrDefault(string? value)
    {
        if (string.Equals(value, "en", StringComparison.OrdinalIgnoreCase))
        {
            return LanguageCode.En;
        }

        if (string.Equals(value, "zh-TW", StringComparison.OrdinalIgnoreCase))
        {
            return LanguageCode.ZhTW;
        }

        return LanguageCode.ZhTW;
    }
}
