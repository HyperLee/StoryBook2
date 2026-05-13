namespace StoryBook.Services;

/// <summary>
/// Resolves and exposes supported browser theme preference metadata.
/// </summary>
public sealed class ThemePreferenceService
{
    private static readonly ThemeModeMetadata[] SupportedModes =
    [
        new(
            "light",
            "亮色模式",
            "Light mode",
            "使用明亮背景閱讀故事。",
            "Read stories with a bright background.",
            1,
            false),
        new(
            "dark",
            "深色模式",
            "Dark mode",
            "使用較暗背景舒適閱讀。",
            "Read comfortably with a darker background.",
            2,
            false),
        new(
            "system",
            "跟隨系統",
            "Use system setting",
            "依照裝置目前的外觀設定切換。",
            "Follow this device's current appearance setting.",
            3,
            true)
    ];

    /// <summary>
    /// Browser localStorage key for the selected theme mode.
    /// </summary>
    public string StorageKey => "storybook.theme";

    /// <summary>
    /// Default selected mode used when no valid preference exists.
    /// </summary>
    public string DefaultMode => "system";

    /// <summary>
    /// Safe effective theme used when the browser system preference is unknown.
    /// </summary>
    public string SafeEffectiveTheme => "light";

    /// <summary>
    /// Supported selectable theme modes in display order.
    /// </summary>
    public IReadOnlyList<ThemeModeMetadata> Modes => SupportedModes;

    /// <summary>
    /// Parses a stored theme mode, falling back to <c>system</c>.
    /// </summary>
    public string ParseMode(string? value)
    {
        string normalized = (value ?? string.Empty).Trim().ToLowerInvariant();
        return SupportedModes.Any(mode => mode.Value == normalized) ? normalized : DefaultMode;
    }

    /// <summary>
    /// Converts a selected mode to the browser storage value, never deriving from the effective theme.
    /// </summary>
    public string ToStorageValue(string? selectedMode)
    {
        return ParseMode(selectedMode);
    }

    /// <summary>
    /// Resolves the effective light or dark theme for a selected mode.
    /// </summary>
    public string ResolveEffectiveTheme(string? selectedMode, bool? prefersDark)
    {
        string mode = ParseMode(selectedMode);

        return mode switch
        {
            "light" => "light",
            "dark" => "dark",
            _ => prefersDark == true ? "dark" : SafeEffectiveTheme
        };
    }
}

/// <summary>
/// Describes a selectable browser theme mode and its bilingual display text.
/// </summary>
public sealed record ThemeModeMetadata(
    string Value,
    string LabelZhTW,
    string LabelEn,
    string DescriptionZhTW,
    string DescriptionEn,
    int SortOrder,
    bool IsDefault)
{
    /// <summary>
    /// Returns the display label for a supported language, falling back to Traditional Chinese.
    /// </summary>
    public string GetLabel(string? language)
    {
        return GetLocalizedValue(language, LabelZhTW, LabelEn);
    }

    /// <summary>
    /// Returns the short description for a supported language, falling back to Traditional Chinese.
    /// </summary>
    public string GetDescription(string? language)
    {
        return GetLocalizedValue(language, DescriptionZhTW, DescriptionEn);
    }

    private static string GetLocalizedValue(string? language, string zhTW, string en)
    {
        string value = string.Equals(language, "en", StringComparison.OrdinalIgnoreCase) ? en : zhTW;
        return string.IsNullOrWhiteSpace(value) ? zhTW : value;
    }
}
