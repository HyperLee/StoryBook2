using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StoryBook.Models;
using StoryBook.Services;
using StoryBook.Tests.Support;

namespace StoryBook.Tests.Unit;

public sealed class ComparisonCatalogServiceTests
{
    [Fact]
    public void GetSnapshot_projects_full_candidate_set_field_definitions_statuses_and_detail_links()
    {
        ComparisonCatalogService service = CreateService();

        ComparisonCatalogSnapshot snapshot = service.GetSnapshot();

        Assert.Equal(23, snapshot.Candidates.Count);
        Assert.True(snapshot.HasEnoughCandidates);
        Assert.False(snapshot.HasPartialFailure);
        Assert.False(snapshot.HasAllSourcesFailed);
        Assert.Equal(2, snapshot.SourceStatuses.Count);
        Assert.All(snapshot.SourceStatuses, status => Assert.True(status.IsAvailable));
        Assert.Collection(
            snapshot.FieldDefinitions.Select(field => field.Code),
            code => Assert.Equal("source", code),
            code => Assert.Equal("name", code),
            code => Assert.Equal("diet", code),
            code => Assert.Equal("living-area", code),
            code => Assert.Equal("period", code),
            code => Assert.Equal("discovery-location", code),
            code => Assert.Equal("summary", code),
            code => Assert.Equal("detail-link", code));

        ComparisonCandidate tyrannosaurus = Assert.Single(snapshot.Candidates, item => item.StableId == "dinosaurs:tyrannosaurus-rex");
        Assert.Equal(ExplorationSourceType.Dinosaurs, tyrannosaurus.Source);
        Assert.Equal("/dinosaurs/tyrannosaurus-rex", tyrannosaurus.DetailHref);
        Assert.Equal("恐龍", tyrannosaurus.GetSourceLabel(LanguageCode.ZhTW));
        Assert.Equal("暴龍", tyrannosaurus.GetName(LanguageCode.ZhTW));
        Assert.Equal("肉食性", tyrannosaurus.GetDiet(LanguageCode.ZhTW));
        Assert.Equal("白堊紀晚期", tyrannosaurus.GetFieldValue("period").GetText(LanguageCode.ZhTW));
        Assert.Equal(ComparisonFieldValueState.NotApplicable, tyrannosaurus.GetFieldValue("living-area").State);

        ComparisonCandidate shark = Assert.Single(snapshot.Candidates, item => item.StableId == "aquarium:shark");
        Assert.Equal(ExplorationSourceType.Aquarium, shark.Source);
        Assert.Equal("/aquarium/shark", shark.DetailHref);
        Assert.Equal("水族館", shark.GetSourceLabel(LanguageCode.ZhTW));
        Assert.Equal("鯊魚", shark.GetName(LanguageCode.ZhTW));
        Assert.NotEmpty(shark.GetFieldValue("living-area").GetText(LanguageCode.ZhTW));
        Assert.Equal(ComparisonFieldValueState.NotApplicable, shark.GetFieldValue("period").State);
    }

    [Fact]
    public void GetSnapshot_uses_stable_id_source_order_source_sort_order_and_existing_detail_hrefs()
    {
        ComparisonCatalogService service = CreateService();

        ComparisonCatalogSnapshot snapshot = service.GetSnapshot();

        Assert.Equal(
            snapshot.Candidates.OrderBy(item => item.Source.GetSortOrder()).ThenBy(item => item.SortOrder).Select(item => item.StableId),
            snapshot.Candidates.Select(item => item.StableId));
        Assert.All(snapshot.Candidates, candidate =>
        {
            Assert.Matches("^(dinosaurs|aquarium):[a-z0-9]+(?:-[a-z0-9]+)*$", candidate.StableId);
            Assert.StartsWith($"/{candidate.Source.GetRoutePrefix()}/", candidate.DetailHref, StringComparison.Ordinal);
            Assert.Equal(candidate.Source.GetSortOrder(), candidate.SourceSortOrder);
        });
        Assert.Equal(8, snapshot.Candidates.Count(candidate => candidate.Source == ExplorationSourceType.Dinosaurs));
        Assert.Equal(15, snapshot.Candidates.Count(candidate => candidate.Source == ExplorationSourceType.Aquarium));
    }

    [Fact]
    public void GetSnapshot_reports_partial_source_failure_with_available_candidates_and_sanitized_warning()
    {
        RecordingLogger<ComparisonCatalogService> logger = new();
        ComparisonCatalogService service = CreateService(
            logger: logger,
            aquariumContentPath: "Data/not-found-aquarium.json");

        ComparisonCatalogSnapshot snapshot = service.GetSnapshot();

        Assert.True(snapshot.HasPartialFailure);
        Assert.False(snapshot.HasAllSourcesFailed);
        Assert.True(snapshot.HasEnoughCandidates);
        Assert.NotEmpty(snapshot.Candidates);
        Assert.DoesNotContain(snapshot.Candidates, candidate => candidate.Source == ExplorationSourceType.Aquarium);
        ComparisonSourceStatus aquarium = Assert.Single(snapshot.SourceStatuses, status => status.Source == ExplorationSourceType.Aquarium);
        Assert.False(aquarium.IsAvailable);
        Assert.Equal(0, aquarium.CandidateCount);
        Assert.False(string.IsNullOrWhiteSpace(aquarium.GetFriendlyMessage(LanguageCode.ZhTW)));
        Assert.True(logger.Contains(LogLevel.Warning, "aquarium"));
        Assert.DoesNotContain(logger.Entries, entry => entry.Message.Contains("not-found-aquarium.json", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(logger.Entries, entry => entry.Message.Contains("secret", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void GetSnapshot_reports_all_sources_failed_with_friendly_statuses()
    {
        ComparisonCatalogService service = CreateService(
            dinosaurContentPath: "Data/not-found-dinosaurs.json",
            aquariumContentPath: "Data/not-found-aquarium.json");

        ComparisonCatalogSnapshot snapshot = service.GetSnapshot();

        Assert.Empty(snapshot.Candidates);
        Assert.False(snapshot.HasEnoughCandidates);
        Assert.False(snapshot.HasPartialFailure);
        Assert.True(snapshot.HasAllSourcesFailed);
        Assert.All(snapshot.SourceStatuses, status =>
        {
            Assert.False(status.IsAvailable);
            Assert.Equal(0, status.CandidateCount);
            Assert.False(string.IsNullOrWhiteSpace(status.GetSourceLabel(LanguageCode.ZhTW)));
            Assert.False(string.IsNullOrWhiteSpace(status.GetFriendlyMessage(LanguageCode.ZhTW)));
            Assert.False(string.IsNullOrWhiteSpace(status.GetFriendlyMessage(LanguageCode.En)));
        });
    }

    [Fact]
    public void Snapshot_detects_fewer_than_two_candidates_as_not_enough()
    {
        ComparisonCatalogSnapshot snapshot = new(
            [CreateCandidate("dinosaurs:tyrannosaurus-rex")],
            [CreateFieldDefinition("name")],
            [new ComparisonSourceStatus { Source = ExplorationSourceType.Dinosaurs, IsAvailable = true, CandidateCount = 1 }]);

        Assert.False(snapshot.HasEnoughCandidates);
        Assert.False(snapshot.HasAllSourcesFailed);
    }

    [Fact]
    public void Localized_field_values_and_labels_fallback_to_nonblank_zhTW_values()
    {
        ComparisonFieldValue value = new()
        {
            State = ComparisonFieldValueState.Available,
            TextZhTW = "繁體中文值",
            TextEn = ""
        };
        ComparisonFieldDefinition definition = new()
        {
            Code = "period",
            LabelZhTW = "生活時期",
            LabelEn = "",
            NotApplicableTextZhTW = "這一欄對牠不適用",
            NotApplicableTextEn = ""
        };
        ComparisonCandidate candidate = CreateCandidate("aquarium:shark", nameEn: "", summaryEn: "", dietEn: "", discoveryLocationsEn: "");

        Assert.Equal("繁體中文值", value.GetText(LanguageCode.En));
        Assert.Equal("生活時期", definition.GetLabel(LanguageCode.En));
        Assert.Equal("這一欄對牠不適用", definition.GetNotApplicableText(LanguageCode.En));
        Assert.Equal("鯊魚", candidate.GetName(LanguageCode.En));
        Assert.False(string.IsNullOrWhiteSpace(candidate.GetSummary(LanguageCode.En)));
        Assert.False(string.IsNullOrWhiteSpace(candidate.GetDiet(LanguageCode.En)));
        Assert.False(string.IsNullOrWhiteSpace(candidate.GetDiscoveryLocations(LanguageCode.En)));
    }

    private static ComparisonCatalogService CreateService(
        RecordingLogger<ComparisonCatalogService>? logger = null,
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

        return new ComparisonCatalogService(
            dinosaurCatalog,
            aquariumCatalog,
            logger ?? new RecordingLogger<ComparisonCatalogService>());
    }

    private static ComparisonCandidate CreateCandidate(
        string stableId,
        string nameEn = "Shark",
        string summaryEn = "A quick swimmer.",
        string dietEn = "Carnivore",
        string discoveryLocationsEn = "Many oceans")
    {
        return new ComparisonCandidate
        {
            StableId = stableId,
            Source = stableId.StartsWith("aquarium:", StringComparison.Ordinal)
                ? ExplorationSourceType.Aquarium
                : ExplorationSourceType.Dinosaurs,
            Slug = stableId.Split(':')[1],
            DetailHref = "/aquarium/shark",
            SortOrder = 1,
            SourceSortOrder = 2,
            SourceLabelZhTW = "水族館",
            SourceLabelEn = "Aquarium",
            NameZhTW = "鯊魚",
            NameEn = nameEn,
            SummaryZhTW = "鯊魚在海裡游泳。",
            SummaryEn = summaryEn,
            DietZhTW = "肉食性",
            DietEn = dietEn,
            DiscoveryLocationsZhTW = "許多海洋",
            DiscoveryLocationsEn = discoveryLocationsEn,
            FieldValues = new Dictionary<string, ComparisonFieldValue>(StringComparer.Ordinal)
            {
                ["name"] = new()
                {
                    State = ComparisonFieldValueState.Available,
                    TextZhTW = "鯊魚",
                    TextEn = nameEn
                }
            }
        };
    }

    private static ComparisonFieldDefinition CreateFieldDefinition(string code)
    {
        return new ComparisonFieldDefinition
        {
            Code = code,
            LabelZhTW = "名稱",
            LabelEn = "Name",
            NotApplicableTextZhTW = "這一欄對牠不適用",
            NotApplicableTextEn = "This field does not fit this friend."
        };
    }
}
