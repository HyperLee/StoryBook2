using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StoryBook.Models;
using StoryBook.Services;
using StoryBook.Tests.Support;

namespace StoryBook.Tests.Unit;

public sealed class ExplorationCatalogServiceTests
{
    [Fact]
    public void GetSnapshot_projects_sources_statuses_and_stable_detail_links()
    {
        ExplorationCatalogService service = CreateService();

        var snapshot = service.GetSnapshot();

        Assert.Equal(23, snapshot.Items.Count);
        Assert.Equal(2, snapshot.SourceStatuses.Count);
        Assert.All(snapshot.SourceStatuses, status => Assert.True(status.IsAvailable));
        Assert.Equal(
            snapshot.Items.OrderBy(item => item.Source.GetSortOrder()).ThenBy(item => item.SortOrder).Select(item => item.StableId),
            snapshot.Items.Select(item => item.StableId));

        ExplorationItem tyrannosaurus = Assert.Single(snapshot.Items, item => item.StableId == "dinosaurs:tyrannosaurus-rex");
        Assert.Equal(ExplorationSourceType.Dinosaurs, tyrannosaurus.Source);
        Assert.Equal("/dinosaurs/tyrannosaurus-rex", tyrannosaurus.DetailHref);
        Assert.Contains("暴龍", tyrannosaurus.SearchText);
        Assert.Contains("Tyrannosaurus Rex", tyrannosaurus.SearchText);
        Assert.Contains(tyrannosaurus.Facets, facet => facet.GroupCode == "source" && facet.ValueCode == "dinosaurs");
        Assert.Contains(tyrannosaurus.Facets, facet => facet.GroupCode == "diet" && facet.ValueCode == "carnivore");

        ExplorationItem clownfish = Assert.Single(snapshot.Items, item => item.StableId == "aquarium:clownfish");
        Assert.Equal(ExplorationSourceType.Aquarium, clownfish.Source);
        Assert.Equal("/aquarium/clownfish", clownfish.DetailHref);
        Assert.Contains("小丑魚", clownfish.SearchText);
        Assert.Contains("Clownfish", clownfish.SearchText);
        Assert.Contains(clownfish.Facets, facet => facet.GroupCode == "living-area" && facet.ValueCode == "coral-reef");
    }

    [Fact]
    public void GetSnapshot_reports_partial_source_failure_without_sensitive_log_content()
    {
        RecordingLogger<ExplorationCatalogService> logger = new();
        ExplorationCatalogService service = CreateService(
            logger: logger,
            aquariumContentPath: "Data/not-found-aquarium.json");

        var snapshot = service.GetSnapshot();

        Assert.NotEmpty(snapshot.Items);
        Assert.Contains(snapshot.Items, item => item.Source == ExplorationSourceType.Dinosaurs);
        Assert.DoesNotContain(snapshot.Items, item => item.Source == ExplorationSourceType.Aquarium);
        ExplorationSourceStatus aquarium = Assert.Single(snapshot.SourceStatuses, status => status.Source == ExplorationSourceType.Aquarium);
        Assert.False(aquarium.IsAvailable);
        Assert.Equal(0, aquarium.ItemCount);
        Assert.False(string.IsNullOrWhiteSpace(aquarium.GetFriendlyMessage(LanguageCode.ZhTW)));
        Assert.True(logger.Contains(LogLevel.Warning, "aquarium"));
        Assert.DoesNotContain(logger.Entries, entry => entry.Message.Contains("not-found-aquarium.json", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(logger.Entries, entry => entry.Message.Contains("shark", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(logger.Entries, entry => entry.Message.Contains("secret", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void GetSnapshot_reports_all_failed_sources_with_friendly_statuses()
    {
        ExplorationCatalogService service = CreateService(
            dinosaurContentPath: "Data/not-found-dinosaurs.json",
            aquariumContentPath: "Data/not-found-aquarium.json");

        var snapshot = service.GetSnapshot();

        Assert.Empty(snapshot.Items);
        Assert.Equal(2, snapshot.SourceStatuses.Count);
        Assert.All(snapshot.SourceStatuses, status =>
        {
            Assert.False(status.IsAvailable);
            Assert.Equal(0, status.ItemCount);
            Assert.False(string.IsNullOrWhiteSpace(status.GetFriendlyMessage(LanguageCode.ZhTW)));
            Assert.False(string.IsNullOrWhiteSpace(status.GetFriendlyMessage(LanguageCode.En)));
        });
    }

    [Fact]
    public void Facet_groups_include_source_diet_living_area_period_and_discovery_location()
    {
        ExplorationCatalogService service = CreateService();

        var snapshot = service.GetSnapshot();

        Assert.Collection(
            snapshot.FacetGroups.Select(group => group.Code),
            code => Assert.Equal("source", code),
            code => Assert.Equal("diet", code),
            code => Assert.Equal("living-area", code),
            code => Assert.Equal("period", code),
            code => Assert.Equal("discovery-location", code));

        Assert.All(snapshot.FacetGroups, group =>
        {
            Assert.Equal("single", group.SelectionMode);
            Assert.NotEmpty(group.Values);
            Assert.Equal(
                group.Values.OrderBy(value => value.SortOrder).ThenBy(value => value.ValueCode).Select(value => value.ValueCode),
                group.Values.Select(value => value.ValueCode));
        });
    }

    [Fact]
    public void Projection_facet_groups_and_statuses_have_nonblank_zhTW_fallbacks()
    {
        ExplorationCatalogService service = CreateService();

        var snapshot = service.GetSnapshot();

        Assert.All(snapshot.Items, item =>
        {
            Assert.False(string.IsNullOrWhiteSpace(item.GetName(LanguageCode.ZhTW)));
            Assert.False(string.IsNullOrWhiteSpace(item.GetSummary(LanguageCode.ZhTW)));
            Assert.False(string.IsNullOrWhiteSpace(item.GetSourceLabel(LanguageCode.ZhTW)));
            Assert.False(string.IsNullOrWhiteSpace(item.GetImageAltText(LanguageCode.ZhTW)));
        });

        Assert.All(snapshot.FacetGroups, group =>
        {
            Assert.False(string.IsNullOrWhiteSpace(group.GetLabel(LanguageCode.ZhTW)));
            Assert.All(group.Values, value => Assert.False(string.IsNullOrWhiteSpace(value.GetLabel(LanguageCode.ZhTW))));
        });

        Assert.All(snapshot.SourceStatuses, status =>
        {
            Assert.False(string.IsNullOrWhiteSpace(status.GetSourceLabel(LanguageCode.ZhTW)));
            if (!status.IsAvailable)
            {
                Assert.False(string.IsNullOrWhiteSpace(status.GetFriendlyMessage(LanguageCode.ZhTW)));
            }
        });
    }

    private static ExplorationCatalogService CreateService(
        RecordingLogger<ExplorationCatalogService>? logger = null,
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

        return new ExplorationCatalogService(
            dinosaurCatalog,
            aquariumCatalog,
            logger ?? new RecordingLogger<ExplorationCatalogService>());
    }
}
