using Microsoft.Extensions.Options;
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
}
