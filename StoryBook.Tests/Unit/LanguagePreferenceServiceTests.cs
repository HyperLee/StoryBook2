using StoryBook.Models;
using StoryBook.Services;

namespace StoryBook.Tests.Unit;

public sealed class LanguagePreferenceServiceTests
{
    [Theory]
    [InlineData("zh-TW", LanguageCode.ZhTW)]
    [InlineData("en", LanguageCode.En)]
    [InlineData("EN", LanguageCode.En)]
    public void Parse_returns_supported_language_codes(string value, LanguageCode expected)
    {
        LanguagePreferenceService service = new();

        Assert.Equal(expected, service.Parse(value));
    }

    [Theory]
    [InlineData("")]
    [InlineData("fr")]
    [InlineData("zh")]
    [InlineData(null)]
    public void Parse_falls_back_to_default_language_for_invalid_values(string? value)
    {
        LanguagePreferenceService service = new();

        Assert.Equal(LanguageCode.ZhTW, service.Parse(value));
    }

    [Fact]
    public void Service_exposes_default_language_and_local_storage_key()
    {
        LanguagePreferenceService service = new();

        Assert.Equal(LanguageCode.ZhTW, service.DefaultLanguage);
        Assert.Equal("storybook.language", service.StorageKey);
        Assert.Equal("zh-TW", service.ToStorageValue(LanguageCode.ZhTW));
        Assert.Equal("en", service.ToStorageValue(LanguageCode.En));
    }

    [Fact]
    public void Missing_localized_display_text_falls_back_without_blank_content()
    {
        DinosaurText missingEnglish = new()
        {
            ZhTW = "繁體中文",
            En = ""
        };

        DinosaurText missingChinese = new()
        {
            ZhTW = "",
            En = "English text"
        };

        Assert.Equal("繁體中文", missingEnglish.Get(LanguageCode.En));
        Assert.Equal("English text", missingChinese.Get(LanguageCode.ZhTW));
    }

    [Fact]
    public void Aquarium_text_uses_shared_storybook_language_fallback()
    {
        AquariumText missingEnglish = new()
        {
            ZhTW = "水族館朋友",
            En = ""
        };

        Assert.Equal("水族館朋友", missingEnglish.Get(LanguageCode.En));
        Assert.Equal("水族館朋友", missingEnglish.Get("invalid"));
    }
}
