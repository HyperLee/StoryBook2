using StoryBook.Models;
using StoryBook.Services;

namespace StoryBook.Tests.Unit;

public sealed class LearningJourneyContentValidationTests
{
    [Fact]
    public void ValidateCatalog_reports_slug_text_goal_minutes_source_and_reference_sort_order_errors()
    {
        LearningJourneyContentValidator validator = new();
        JourneyCatalog catalog = new()
        {
            Journeys =
            [
                CreateJourney(
                    slug: "Bad Slug",
                    sortOrder: 0,
                    title: new JourneyText { ZhTW = "", En = "" },
                    goals: [],
                    minutes: 0,
                    ageGuidance: new JourneyText { ZhTW = "" },
                    references:
                    [
                        Ref("space", "bad slug", 0)
                    ]),
                CreateJourney(slug: "duplicate-slug"),
                CreateJourney(slug: "duplicate-slug")
            ]
        };

        LearningJourneyContentValidationResult result = validator.ValidateCatalog(catalog);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.Contains("slug", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(result.Errors, error => error.Contains("duplicate", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(result.Errors, error => error.Contains("title", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(result.Errors, error => error.Contains("learningGoals", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(result.Errors, error => error.Contains("suggestedReadingMinutes", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(result.Errors, error => error.Contains("ageGuidance", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(result.Errors, error => error.Contains("source", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(result.Errors, error => error.Contains("sortOrder", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void JourneyText_falls_back_to_zhTW_for_missing_invalid_or_blank_localized_values()
    {
        JourneyText text = new()
        {
            ZhTW = "安全文字",
            En = ""
        };

        Assert.Equal("安全文字", text.Get(LanguageCode.En));
        Assert.Equal("安全文字", text.Get(LanguageCode.ZhTW));
        Assert.Equal("安全文字", text.Get("not-a-language"));
    }

    [Fact]
    public void JourneyStoryItem_localized_display_values_fall_back_without_blanks()
    {
        JourneyStoryItem item = new()
        {
            SourceLabelZhTW = "恐龍",
            SourceLabelEn = "",
            NameZhTW = "暴龍",
            NameEn = "",
            SummaryZhTW = "暴龍用大腳印帶朋友走到陽光下。",
            SummaryEn = "",
            ImageAltTextZhTW = "暴龍和小動物在大腳印旁",
            ImageAltTextEn = ""
        };

        Assert.Equal("恐龍", item.GetSourceLabel(LanguageCode.En));
        Assert.Equal("暴龍", item.GetName(LanguageCode.En));
        Assert.False(string.IsNullOrWhiteSpace(item.GetSummary(LanguageCode.En)));
        Assert.False(string.IsNullOrWhiteSpace(item.GetImageAltText(LanguageCode.En)));
    }

    private static LearningJourney CreateJourney(
        string slug,
        int sortOrder = 1,
        JourneyText? title = null,
        List<JourneyText>? goals = null,
        int minutes = 10,
        JourneyText? ageGuidance = null,
        List<JourneyStoryReference>? references = null)
    {
        return new LearningJourney
        {
            Slug = slug,
            SortOrder = sortOrder,
            Title = title ?? Text("旅程", "Journey"),
            Summary = Text("旅程摘要", "Journey summary"),
            LearningGoals = goals ?? [Text("學習目標", "Learning goal")],
            SuggestedReadingMinutes = minutes,
            AgeGuidance = ageGuidance ?? Text("5-7 歲", "Ages 5-7"),
            StoryReferences = references ??
            [
                Ref("dinosaurs", "tyrannosaurus-rex", 1),
                Ref("dinosaurs", "triceratops", 2),
                Ref("aquarium", "shark", 3)
            ]
        };
    }

    private static JourneyStoryReference Ref(string source, string slug, int sortOrder)
    {
        return new JourneyStoryReference
        {
            Source = source,
            Slug = slug,
            SortOrder = sortOrder
        };
    }

    private static JourneyText Text(string zhTW, string en)
    {
        return new JourneyText
        {
            ZhTW = zhTW,
            En = en
        };
    }
}
