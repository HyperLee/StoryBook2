using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StoryBook.Models;
using StoryBook.Services;
using StoryBook.Tests.Support;

namespace StoryBook.Tests.Unit;

public sealed class PassportCatalogServiceTests
{
    [Fact]
    public void GetSnapshot_projects_full_story_set_badges_statuses_and_detail_links()
    {
        PassportCatalogService service = CreateService();

        PassportCatalogSnapshot snapshot = service.GetSnapshot();

        Assert.Equal(23, snapshot.TotalStoryCount);
        Assert.True(snapshot.HasAnyStory);
        Assert.False(snapshot.HasPartialSourceFailure);
        Assert.False(snapshot.HasAllSourcesFailed);
        Assert.Equal(5, snapshot.Badges.Count);
        Assert.Equal(new[] { "first-story", "three-stories", "all-dinosaurs", "all-aquarium", "all-stories" }, snapshot.Badges.Select(badge => badge.Code));
        Assert.Equal(2, snapshot.SourceStatuses.Count);
        Assert.All(snapshot.SourceStatuses, status => Assert.True(status.IsAvailable));
        Assert.Equal(8, snapshot.Stories.Count(story => story.SourceCode == "dinosaurs"));
        Assert.Equal(15, snapshot.Stories.Count(story => story.SourceCode == "aquarium"));

        PassportStoryItem triceratops = Assert.Single(snapshot.Stories, story => story.StableId == "dinosaurs:triceratops");
        Assert.Equal(ExplorationSourceType.Dinosaurs, triceratops.Source);
        Assert.Equal("/dinosaurs/triceratops", triceratops.DetailHref);
        Assert.Equal("恐龍", triceratops.GetSourceLabel(LanguageCode.ZhTW));
        Assert.Equal("Dinosaurs", triceratops.GetSourceLabel(LanguageCode.En));
        Assert.Equal("三角龍", triceratops.GetName(LanguageCode.ZhTW));
        Assert.Equal("Triceratops", triceratops.GetName(LanguageCode.En));
        Assert.False(string.IsNullOrWhiteSpace(triceratops.GetSummary(LanguageCode.ZhTW)));
        Assert.False(string.IsNullOrWhiteSpace(triceratops.GetSummary(LanguageCode.En)));

        PassportStoryItem seaTurtle = Assert.Single(snapshot.Stories, story => story.StableId == "aquarium:sea-turtle");
        Assert.Equal(ExplorationSourceType.Aquarium, seaTurtle.Source);
        Assert.Equal("/aquarium/sea-turtle", seaTurtle.DetailHref);
        Assert.Equal("水族館", seaTurtle.GetSourceLabel(LanguageCode.ZhTW));
        Assert.Equal("Aquarium", seaTurtle.GetSourceLabel(LanguageCode.En));
        Assert.Equal("海龜", seaTurtle.GetName(LanguageCode.ZhTW));
        Assert.Equal("Sea turtle", seaTurtle.GetName(LanguageCode.En));
    }

    [Fact]
    public void GetSnapshot_uses_dinosaur_before_aquarium_order_and_safe_nonblank_text()
    {
        PassportCatalogService service = CreateService();

        PassportCatalogSnapshot snapshot = service.GetSnapshot();

        Assert.Equal(
            snapshot.Stories.OrderBy(story => story.SourceSortOrder).ThenBy(story => story.SortOrder).ThenBy(story => story.StableId).Select(story => story.StableId),
            snapshot.Stories.Select(story => story.StableId));
        Assert.StartsWith("dinosaurs:", snapshot.Stories.First().StableId, StringComparison.Ordinal);
        Assert.StartsWith("aquarium:", snapshot.Stories.Last().StableId, StringComparison.Ordinal);
        Assert.All(snapshot.Stories, story =>
        {
            Assert.Matches("^(dinosaurs|aquarium):[a-z0-9]+(?:-[a-z0-9]+)*$", story.StableId);
            Assert.StartsWith($"/{story.Source.GetRoutePrefix()}/", story.DetailHref, StringComparison.Ordinal);
            Assert.Equal(story.Source.GetSortOrder(), story.SourceSortOrder);
            Assert.False(string.IsNullOrWhiteSpace(story.GetName(LanguageCode.ZhTW)));
            Assert.False(string.IsNullOrWhiteSpace(story.GetName(LanguageCode.En)));
            Assert.False(string.IsNullOrWhiteSpace(story.GetSummary(LanguageCode.ZhTW)));
            Assert.False(string.IsNullOrWhiteSpace(story.GetSummary(LanguageCode.En)));
            Assert.False(string.IsNullOrWhiteSpace(story.GetSourceLabel(LanguageCode.ZhTW)));
            Assert.False(string.IsNullOrWhiteSpace(story.GetSourceLabel(LanguageCode.En)));
        });
    }

    [Fact]
    public void GetSnapshot_reports_partial_source_failure_with_available_stories_and_sanitized_warning()
    {
        RecordingLogger<PassportCatalogService> logger = new();
        PassportCatalogService service = CreateService(
            logger: logger,
            aquariumContentPath: "Data/not-found-aquarium.json");

        PassportCatalogSnapshot snapshot = service.GetSnapshot();

        Assert.True(snapshot.HasPartialSourceFailure);
        Assert.False(snapshot.HasAllSourcesFailed);
        Assert.True(snapshot.HasAnyStory);
        Assert.DoesNotContain(snapshot.Stories, story => story.Source == ExplorationSourceType.Aquarium);
        PassportSourceStatus aquarium = Assert.Single(snapshot.SourceStatuses, status => status.Source == ExplorationSourceType.Aquarium);
        Assert.False(aquarium.IsAvailable);
        Assert.Equal("aquarium", aquarium.SourceCode);
        Assert.Equal(0, aquarium.StoryCount);
        Assert.Equal("source-load-failed", aquarium.ReasonCode);
        Assert.False(string.IsNullOrWhiteSpace(aquarium.GetFriendlyMessage(LanguageCode.ZhTW)));
        Assert.True(logger.Contains(LogLevel.Warning, "aquarium"));
        Assert.DoesNotContain(logger.Entries, entry => entry.Message.Contains("not-found-aquarium.json", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void GetSnapshot_reports_all_sources_failed_with_friendly_statuses()
    {
        PassportCatalogService service = CreateService(
            dinosaurContentPath: "Data/not-found-dinosaurs.json",
            aquariumContentPath: "Data/not-found-aquarium.json");

        PassportCatalogSnapshot snapshot = service.GetSnapshot();

        Assert.Empty(snapshot.Stories);
        Assert.Equal(0, snapshot.TotalStoryCount);
        Assert.False(snapshot.HasAnyStory);
        Assert.False(snapshot.HasPartialSourceFailure);
        Assert.True(snapshot.HasAllSourcesFailed);
        Assert.All(snapshot.SourceStatuses, status =>
        {
            Assert.False(status.IsAvailable);
            Assert.Equal(0, status.StoryCount);
            Assert.False(string.IsNullOrWhiteSpace(status.SourceCode));
            Assert.False(string.IsNullOrWhiteSpace(status.ReasonCode));
            Assert.False(string.IsNullOrWhiteSpace(status.GetFriendlyMessage(LanguageCode.ZhTW)));
            Assert.False(string.IsNullOrWhiteSpace(status.GetFriendlyMessage(LanguageCode.En)));
        });
    }

    [Fact]
    public void Passport_story_item_falls_back_to_nonblank_zhTW_values()
    {
        PassportStoryItem story = new()
        {
            StableId = "aquarium:shark",
            Source = ExplorationSourceType.Aquarium,
            SourceCode = "aquarium",
            Slug = "shark",
            DetailHref = "/aquarium/shark",
            SourceLabelZhTW = "水族館",
            SourceLabelEn = "",
            NameZhTW = "鯊魚",
            NameEn = "",
            SummaryZhTW = "鯊魚在海裡游泳。",
            SummaryEn = ""
        };

        Assert.Equal("水族館", story.GetSourceLabel(LanguageCode.En));
        Assert.Equal("鯊魚", story.GetName(LanguageCode.En));
        Assert.Equal("鯊魚在海裡游泳。", story.GetSummary(LanguageCode.En));
    }

    private static PassportCatalogService CreateService(
        RecordingLogger<PassportCatalogService>? logger = null,
        string dinosaurContentPath = "Data/dinosaurs.json",
        string aquariumContentPath = "Data/aquarium.json")
    {
        DinosaurCatalogService dinosaurCatalog = new(
            Options.Create(new DinosaurCatalogOptions { ContentPath = dinosaurContentPath }),
            new FakeWebHostEnvironment(TestPaths.StoryBookRoot),
            new DinosaurContentValidator(),
            new RecordingLogger<DinosaurCatalogService>());

        AquariumCatalogService aquariumCatalog = new(
            Options.Create(new AquariumCatalogOptions { ContentPath = aquariumContentPath }),
            new FakeWebHostEnvironment(TestPaths.StoryBookRoot),
            new AquariumContentValidator(),
            new RecordingLogger<AquariumCatalogService>());

        return new PassportCatalogService(
            dinosaurCatalog,
            aquariumCatalog,
            new PassportPreferenceService(),
            logger ?? new RecordingLogger<PassportCatalogService>());
    }
}
