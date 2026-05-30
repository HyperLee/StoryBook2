using StoryBook.Models;
using StoryBook.Services;

namespace StoryBook.Tests.Unit;

public sealed class PassportPreferenceServiceTests
{
    [Fact]
    public void Service_exposes_storage_key_state_version_and_allowed_sources()
    {
        PassportPreferenceService service = new();

        Assert.Equal("storybook.passport", service.StorageKey);
        Assert.Equal(1, service.StateVersion);
        Assert.Equal(new[] { "dinosaurs", "aquarium" }, service.AllowedSourceCodes);
    }

    [Fact]
    public void Service_exposes_five_fixed_badges_with_bilingual_metadata()
    {
        PassportPreferenceService service = new();

        Assert.Collection(
            service.Badges,
            badge =>
            {
                Assert.Equal("first-story", badge.Code);
                Assert.Equal(PassportBadgeMilestone.CompletedCountAtLeast, badge.Milestone);
                Assert.Equal(1, badge.TargetCount);
                Assert.Null(badge.SourceCode);
                Assert.Equal("讀完第一篇", badge.GetLabel(LanguageCode.ZhTW));
                Assert.Equal("First story", badge.GetLabel(LanguageCode.En));
            },
            badge =>
            {
                Assert.Equal("three-stories", badge.Code);
                Assert.Equal(PassportBadgeMilestone.CompletedCountAtLeast, badge.Milestone);
                Assert.Equal(3, badge.TargetCount);
                Assert.Null(badge.SourceCode);
            },
            badge =>
            {
                Assert.Equal("all-dinosaurs", badge.Code);
                Assert.Equal(PassportBadgeMilestone.CompletedAllInSource, badge.Milestone);
                Assert.Equal("dinosaurs", badge.SourceCode);
                Assert.Null(badge.TargetCount);
            },
            badge =>
            {
                Assert.Equal("all-aquarium", badge.Code);
                Assert.Equal(PassportBadgeMilestone.CompletedAllInSource, badge.Milestone);
                Assert.Equal("aquarium", badge.SourceCode);
                Assert.Null(badge.TargetCount);
            },
            badge =>
            {
                Assert.Equal("all-stories", badge.Code);
                Assert.Equal(PassportBadgeMilestone.CompletedAllStories, badge.Milestone);
                Assert.Null(badge.SourceCode);
                Assert.Null(badge.TargetCount);
            });

        Assert.Equal(new[] { 1, 2, 3, 4, 5 }, service.Badges.Select(badge => badge.SortOrder));
        Assert.Equal(service.Badges.Count, service.Badges.Select(badge => badge.Code).Distinct(StringComparer.Ordinal).Count());
        Assert.All(service.Badges, badge =>
        {
            Assert.False(string.IsNullOrWhiteSpace(badge.GetLabel(LanguageCode.ZhTW)));
            Assert.False(string.IsNullOrWhiteSpace(badge.GetLabel(LanguageCode.En)));
            Assert.False(string.IsNullOrWhiteSpace(badge.GetDescription(LanguageCode.ZhTW)));
            Assert.False(string.IsNullOrWhiteSpace(badge.GetDescription(LanguageCode.En)));
        });
    }

    [Fact]
    public void Badge_metadata_falls_back_to_nonblank_zhTW_values()
    {
        PassportBadgeDefinition badge = new()
        {
            Code = "sample",
            Milestone = PassportBadgeMilestone.CompletedCountAtLeast,
            TargetCount = 1,
            SortOrder = 1,
            LabelZhTW = "繁中徽章",
            LabelEn = "",
            DescriptionZhTW = "繁中說明",
            DescriptionEn = ""
        };

        Assert.Equal("繁中徽章", badge.GetLabel(LanguageCode.En));
        Assert.Equal("繁中說明", badge.GetDescription(LanguageCode.En));
    }
}
