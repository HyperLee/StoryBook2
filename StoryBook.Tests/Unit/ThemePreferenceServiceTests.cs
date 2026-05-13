using StoryBook.Services;

namespace StoryBook.Tests.Unit;

public sealed class ThemePreferenceServiceTests
{
    [Fact]
    public void Service_exposes_storage_key_defaults_and_supported_modes()
    {
        ThemePreferenceService service = new();

        Assert.Equal("storybook.theme", service.StorageKey);
        Assert.Equal("system", service.DefaultMode);
        Assert.Equal("light", service.SafeEffectiveTheme);
        Assert.Equal(new[] { "light", "dark", "system" }, service.Modes.Select(mode => mode.Value));
        Assert.Equal("system", Assert.Single(service.Modes, mode => mode.IsDefault).Value);
    }

    [Theory]
    [InlineData("light", "light")]
    [InlineData("dark", "dark")]
    [InlineData("system", "system")]
    [InlineData("LIGHT", "light")]
    [InlineData(" dark ", "dark")]
    public void ParseMode_returns_supported_theme_modes(string value, string expected)
    {
        ThemePreferenceService service = new();

        Assert.Equal(expected, service.ParseMode(value));
    }

    [Theory]
    [InlineData("")]
    [InlineData("blue")]
    [InlineData("auto")]
    [InlineData(null)]
    public void ParseMode_falls_back_to_system_for_invalid_values(string? value)
    {
        ThemePreferenceService service = new();

        Assert.Equal("system", service.ParseMode(value));
    }

    [Theory]
    [InlineData("light", true, "light")]
    [InlineData("dark", false, "dark")]
    [InlineData("system", true, "dark")]
    [InlineData("system", false, "light")]
    [InlineData("system", null, "light")]
    [InlineData("unsupported", true, "dark")]
    public void ResolveEffectiveTheme_uses_safe_light_fallback_when_system_is_unknown(
        string mode,
        bool? prefersDark,
        string expected)
    {
        ThemePreferenceService service = new();

        Assert.Equal(expected, service.ResolveEffectiveTheme(mode, prefersDark));
    }

    [Fact]
    public void Mode_metadata_exposes_bilingual_labels_and_descriptions_without_blank_fallback()
    {
        ThemePreferenceService service = new();

        ThemeModeMetadata light = service.Modes.Single(mode => mode.Value == "light");
        ThemeModeMetadata dark = service.Modes.Single(mode => mode.Value == "dark");
        ThemeModeMetadata system = service.Modes.Single(mode => mode.Value == "system");

        Assert.Equal("亮色模式", light.GetLabel("zh-TW"));
        Assert.Equal("Light mode", light.GetLabel("en"));
        Assert.Equal("深色模式", dark.GetLabel("zh-TW"));
        Assert.Equal("Dark mode", dark.GetLabel("en"));
        Assert.Equal("跟隨系統", system.GetLabel("zh-TW"));
        Assert.Equal("Use system setting", system.GetLabel("en"));
        Assert.NotEmpty(system.GetDescription("zh-TW"));
        Assert.NotEmpty(system.GetDescription("en"));
        Assert.Equal("跟隨系統", system.GetLabel("invalid"));
        Assert.Equal(system.GetDescription("zh-TW"), system.GetDescription("invalid"));
    }
}
