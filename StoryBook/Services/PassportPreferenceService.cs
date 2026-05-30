using StoryBook.Models;

namespace StoryBook.Services;

/// <summary>
/// Exposes reading passport browser storage metadata and fixed badge definitions.
/// </summary>
public sealed class PassportPreferenceService
{
    private static readonly string[] SupportedSourceCodes = ["dinosaurs", "aquarium"];

    private static readonly PassportBadgeDefinition[] FixedBadges =
    [
        new()
        {
            Code = "first-story",
            Milestone = PassportBadgeMilestone.CompletedCountAtLeast,
            TargetCount = 1,
            SortOrder = 1,
            LabelZhTW = "讀完第一篇",
            LabelEn = "First story",
            DescriptionZhTW = "讀完第一位故事朋友，就能蓋上第一個章。",
            DescriptionEn = "Finish one story friend to earn the first stamp."
        },
        new()
        {
            Code = "three-stories",
            Milestone = PassportBadgeMilestone.CompletedCountAtLeast,
            TargetCount = 3,
            SortOrder = 2,
            LabelZhTW = "三篇小探險",
            LabelEn = "Three adventures",
            DescriptionZhTW = "讀完三篇不同故事，護照會亮起新的徽章。",
            DescriptionEn = "Finish three different stories to light up a new badge."
        },
        new()
        {
            Code = "all-dinosaurs",
            Milestone = PassportBadgeMilestone.CompletedAllInSource,
            SourceCode = "dinosaurs",
            SortOrder = 3,
            LabelZhTW = "恐龍全認識",
            LabelEn = "All dinosaurs",
            DescriptionZhTW = "讀完所有恐龍故事朋友。",
            DescriptionEn = "Finish every dinosaur story friend."
        },
        new()
        {
            Code = "all-aquarium",
            Milestone = PassportBadgeMilestone.CompletedAllInSource,
            SourceCode = "aquarium",
            SortOrder = 4,
            LabelZhTW = "水族館全認識",
            LabelEn = "All aquarium friends",
            DescriptionZhTW = "讀完所有水族館故事朋友。",
            DescriptionEn = "Finish every aquarium story friend."
        },
        new()
        {
            Code = "all-stories",
            Milestone = PassportBadgeMilestone.CompletedAllStories,
            SortOrder = 5,
            LabelZhTW = "故事全探險",
            LabelEn = "All stories",
            DescriptionZhTW = "讀完目前所有故事朋友，完成整本探險護照。",
            DescriptionEn = "Finish every current story friend to complete the passport."
        }
    ];

    /// <summary>
    /// Browser localStorage key for reading passport state.
    /// </summary>
    public string StorageKey => "storybook.passport";

    /// <summary>
    /// Current browser state schema version.
    /// </summary>
    public int StateVersion => 1;

    /// <summary>
    /// Supported source codes in browser state order.
    /// </summary>
    public IReadOnlyList<string> AllowedSourceCodes => SupportedSourceCodes;

    /// <summary>
    /// Fixed badge metadata in display order.
    /// </summary>
    public IReadOnlyList<PassportBadgeDefinition> Badges => FixedBadges;
}
