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
