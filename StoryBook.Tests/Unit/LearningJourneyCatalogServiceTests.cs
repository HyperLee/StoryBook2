using Microsoft.Extensions.Options;
using System.Text.Json;
using StoryBook.Models;
using StoryBook.Services;
using StoryBook.Tests.Support;

namespace StoryBook.Tests.Unit;

public sealed class LearningJourneyCatalogServiceTests
{
    [Fact]
    public void GetSnapshot_loads_sorts_and_exposes_at_least_three_available_journeys()
    {
        LearningJourneyCatalogService service = CreateService();

        JourneyCatalogSnapshot snapshot = service.GetSnapshot();

        Assert.True(snapshot.HasAnyAvailableJourney);
        Assert.False(snapshot.HasAllSourcesFailed);
        Assert.True(snapshot.AvailableJourneys.Count >= 3);
        Assert.Equal(
            snapshot.AvailableJourneys.OrderBy(journey => journey.SortOrder).ThenBy(journey => journey.Slug).Select(journey => journey.Slug),
            snapshot.AvailableJourneys.Select(journey => journey.Slug));
        Assert.Contains(snapshot.AvailableJourneys, journey => journey.Slug == "gentle-giants");
        Assert.All(snapshot.AvailableJourneys, journey =>
        {
            Assert.Matches("^[a-z0-9]+(?:-[a-z0-9]+)*$", journey.Slug);
            Assert.InRange(journey.StoryReferences.Count, 3, 5);
            Assert.False(string.IsNullOrWhiteSpace(journey.Title.Get(LanguageCode.ZhTW)));
            Assert.False(string.IsNullOrWhiteSpace(journey.Summary.Get(LanguageCode.ZhTW)));
            Assert.True(journey.SuggestedReadingMinutes > 0);
        });
    }

    [Fact]
    public void GetStoryItems_resolves_source_names_summaries_hrefs_and_first_story_start_href()
    {
        LearningJourneyCatalogService service = CreateService();

        Assert.True(service.TryGetJourneyBySlug("clever-hunters", out LearningJourney? journey));
        IReadOnlyList<JourneyStoryItem> items = service.GetStoryItems("clever-hunters");

        Assert.NotNull(journey);
        Assert.Equal(4, items.Count);
        Assert.Collection(
            items.Select(item => item.StableId),
            stableId => Assert.Equal("dinosaurs:tyrannosaurus-rex", stableId),
            stableId => Assert.Equal("dinosaurs:velociraptor", stableId),
            stableId => Assert.Equal("aquarium:shark", stableId),
            stableId => Assert.Equal("aquarium:octopus", stableId));

        JourneyStoryItem tyrannosaurus = items[0];
        Assert.Equal(ExplorationSourceType.Dinosaurs, tyrannosaurus.Source);
        Assert.Equal("恐龍", tyrannosaurus.GetSourceLabel(LanguageCode.ZhTW));
        Assert.Equal("Dinosaurs", tyrannosaurus.GetSourceLabel(LanguageCode.En));
        Assert.Equal("暴龍", tyrannosaurus.GetName(LanguageCode.ZhTW));
        Assert.Equal("Tyrannosaurus Rex", tyrannosaurus.GetName(LanguageCode.En));
        Assert.Equal("/dinosaurs/tyrannosaurus-rex", tyrannosaurus.DetailHref);
        Assert.False(string.IsNullOrWhiteSpace(tyrannosaurus.GetSummary(LanguageCode.ZhTW)));

        JourneyStoryItem shark = Assert.Single(items, item => item.StableId == "aquarium:shark");
        Assert.Equal(ExplorationSourceType.Aquarium, shark.Source);
        Assert.Equal("水族館", shark.GetSourceLabel(LanguageCode.ZhTW));
        Assert.Equal("Aquarium", shark.GetSourceLabel(LanguageCode.En));
        Assert.Equal("鯊魚", shark.GetName(LanguageCode.ZhTW));
        Assert.Equal("/aquarium/shark", shark.DetailHref);
        Assert.False(string.IsNullOrWhiteSpace(shark.GetSummary(LanguageCode.En)));

        Assert.Equal("/dinosaurs/tyrannosaurus-rex", service.GetStartReadingHref("clever-hunters"));
    }

    [Fact]
    public void GetSnapshot_hides_not_enough_and_too_many_journeys_and_reports_statuses()
    {
        string path = WriteCatalog(
            CreateJourney("ready-trail"),
            CreateJourney("tiny-trail", references:
            [
                Ref("dinosaurs", "tyrannosaurus-rex", 1),
                Ref("aquarium", "shark", 2)
            ]),
            CreateJourney("wide-trail", references:
            [
                Ref("dinosaurs", "tyrannosaurus-rex", 1),
                Ref("dinosaurs", "triceratops", 2),
                Ref("dinosaurs", "stegosaurus", 3),
                Ref("dinosaurs", "brachiosaurus", 4),
                Ref("aquarium", "shark", 5),
                Ref("aquarium", "octopus", 6)
            ]));
        LearningJourneyCatalogService service = CreateService(journeyContentPath: path);

        JourneyCatalogSnapshot snapshot = service.GetSnapshot();

        Assert.Contains(snapshot.AvailableJourneys, journey => journey.Slug == "ready-trail");
        Assert.DoesNotContain(snapshot.AvailableJourneys, journey => journey.Slug == "tiny-trail");
        Assert.DoesNotContain(snapshot.AvailableJourneys, journey => journey.Slug == "wide-trail");
        Assert.Contains(snapshot.UnavailableStatuses, status =>
            status.JourneySlug == "tiny-trail"
            && status.State == JourneyAvailabilityState.NotEnoughItems
            && !status.CanAppearInList);
        Assert.Contains(snapshot.UnavailableStatuses, status =>
            status.JourneySlug == "wide-trail"
            && status.State == JourneyAvailabilityState.TooManyItems
            && !status.CanAppearInList);
    }

    [Fact]
    public void GetStoryItems_filters_duplicate_and_invalid_references_without_broken_links()
    {
        string path = WriteCatalog(CreateJourney("messy-trail", references:
        [
            Ref("dinosaurs", "tyrannosaurus-rex", 1),
            Ref("dinosaurs", "tyrannosaurus-rex", 2),
            Ref("aquarium", "missing-shark", 3),
            Ref("aquarium", "shark", 4)
        ]));
        LearningJourneyCatalogService service = CreateService(journeyContentPath: path);

        IReadOnlyList<JourneyStoryItem> items = service.GetStoryItems("messy-trail");
        JourneyAvailabilityStatus status = service.GetAvailabilityStatus("messy-trail");

        Assert.Equal(["dinosaurs:tyrannosaurus-rex", "aquarium:shark"], items.Select(item => item.StableId));
        Assert.All(items, item => Assert.Matches("^/(dinosaurs|aquarium)/[a-z0-9]+(?:-[a-z0-9]+)*$", item.DetailHref));
        Assert.Equal(JourneyAvailabilityState.NotEnoughItems, status.State);
        Assert.Equal(2, status.ValidItemCount);
        Assert.NotNull(status.DiagnosticSummary);
        Assert.Equal("not-enough-items", status.DiagnosticSummary.ReasonCode);
    }

    [Fact]
    public void GetSnapshot_reports_source_failure_and_all_sources_failed_with_sanitized_diagnostics()
    {
        RecordingLogger<LearningJourneyCatalogService> logger = new();
        LearningJourneyCatalogService partialService = CreateService(
            logger: logger,
            aquariumContentPath: "Data/not-found-aquarium.json");

        JourneyCatalogSnapshot partialSnapshot = partialService.GetSnapshot();

        Assert.True(partialSnapshot.HasPartialSourceFailure);
        Assert.False(partialSnapshot.HasAllSourcesFailed);
        JourneySourceStatus aquarium = Assert.Single(partialSnapshot.SourceStatuses, status => status.Source == ExplorationSourceType.Aquarium);
        Assert.False(aquarium.IsAvailable);
        Assert.Equal("source-load-failed", aquarium.DiagnosticSummary?.ReasonCode);
        Assert.DoesNotContain(logger.Entries, entry => entry.Message.Contains("not-found-aquarium.json", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(logger.Entries, entry => entry.Message.Contains("System.IO", StringComparison.OrdinalIgnoreCase));

        LearningJourneyCatalogService allFailedService = CreateService(
            dinosaurContentPath: "Data/not-found-dinosaurs.json",
            aquariumContentPath: "Data/not-found-aquarium.json");
        JourneyCatalogSnapshot allFailedSnapshot = allFailedService.GetSnapshot();

        Assert.True(allFailedSnapshot.HasAllSourcesFailed);
        Assert.False(allFailedSnapshot.HasAnyAvailableJourney);
    }

    private static LearningJourneyCatalogService CreateService(
        string journeyContentPath = "Data/journeys.json",
        string dinosaurContentPath = "Data/dinosaurs.json",
        string aquariumContentPath = "Data/aquarium.json",
        RecordingLogger<LearningJourneyCatalogService>? logger = null)
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

        return new LearningJourneyCatalogService(
            Options.Create(new LearningJourneyCatalogOptions { ContentPath = journeyContentPath }),
            new FakeWebHostEnvironment(TestPaths.StoryBookRoot),
            new LearningJourneyContentValidator(),
            dinosaurCatalog,
            aquariumCatalog,
            logger ?? new RecordingLogger<LearningJourneyCatalogService>());
    }

    private static string WriteCatalog(params LearningJourney[] journeys)
    {
        string json = JsonSerializer.Serialize(
            new JourneyCatalog { Journeys = journeys.ToList() },
            new JsonSerializerOptions(JsonSerializerDefaults.Web));
        string path = Path.Combine(Path.GetTempPath(), $"storybook-journeys-{Guid.NewGuid():N}.json");
        File.WriteAllText(path, json);
        return path;
    }

    private static LearningJourney CreateJourney(
        string slug,
        List<JourneyStoryReference>? references = null)
    {
        return new LearningJourney
        {
            Slug = slug,
            SortOrder = 1,
            Title = Text("旅程", "Journey"),
            Summary = Text("旅程摘要", "Journey summary"),
            LearningGoals = [Text("學習目標", "Learning goal")],
            SuggestedReadingMinutes = 10,
            AgeGuidance = Text("5-7 歲", "Ages 5-7"),
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
