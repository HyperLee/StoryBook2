using StoryBook.Models;

namespace StoryBook.Services;

/// <summary>
/// Provides story friend comparison candidates and field definitions.
/// </summary>
public interface IComparisonCatalogService
{
    /// <summary>
    /// Builds the current comparison catalog snapshot.
    /// </summary>
    ComparisonCatalogSnapshot GetSnapshot();
}

/// <summary>
/// Composes dinosaur and aquarium catalog services into the comparison page projection.
/// </summary>
public sealed class ComparisonCatalogService : IComparisonCatalogService
{
    private readonly DinosaurCatalogService _dinosaurCatalogService;
    private readonly AquariumCatalogService _aquariumCatalogService;
    private readonly ILogger<ComparisonCatalogService> _logger;

    /// <summary>
    /// Creates the comparison catalog service from the existing storybook catalog services.
    /// </summary>
    public ComparisonCatalogService(
        DinosaurCatalogService dinosaurCatalogService,
        AquariumCatalogService aquariumCatalogService,
        ILogger<ComparisonCatalogService> logger)
    {
        _dinosaurCatalogService = dinosaurCatalogService;
        _aquariumCatalogService = aquariumCatalogService;
        _logger = logger;
    }

    /// <summary>
    /// Builds the current comparison snapshot.
    /// </summary>
    public ComparisonCatalogSnapshot GetSnapshot()
    {
        List<ComparisonCandidate> candidates = [];
        List<ComparisonSourceStatus> statuses = [];

        IReadOnlyList<ComparisonFieldDefinition> fieldDefinitions = BuildFieldDefinitions();
        AddDinosaurCandidates(candidates, statuses, fieldDefinitions);
        AddAquariumCandidates(candidates, statuses, fieldDefinitions);

        IReadOnlyList<ComparisonCandidate> sortedCandidates = candidates
            .OrderBy(candidate => candidate.SourceSortOrder)
            .ThenBy(candidate => candidate.SortOrder)
            .ToList();

        return new ComparisonCatalogSnapshot(
            sortedCandidates,
            fieldDefinitions,
            statuses.OrderBy(status => status.Source.GetSortOrder()).ToList());
    }

    private static IReadOnlyList<ComparisonFieldDefinition> BuildFieldDefinitions()
    {
        return
        [
            CreateField("source", "故事書來源", "Story source", 1),
            CreateField("name", "名稱", "Name", 2),
            CreateField("diet", "食性", "Diet", 3),
            CreateField("living-area", "生活區域", "Living area", 4),
            CreateField("period", "生活時期", "Period", 5),
            CreateField("discovery-location", "發現地點", "Found in", 6),
            CreateField("summary", "小摘要", "Short summary", 7),
            CreateField("detail-link", "閱讀故事", "Read story", 8)
        ];
    }

    private static ComparisonFieldDefinition CreateField(
        string code,
        string labelZhTW,
        string labelEn,
        int sortOrder)
    {
        return new ComparisonFieldDefinition
        {
            Code = code,
            LabelZhTW = labelZhTW,
            LabelEn = labelEn,
            SortOrder = sortOrder,
            NotApplicableTextZhTW = "這一欄對這位故事朋友不適用。",
            NotApplicableTextEn = "This field does not fit this story friend."
        };
    }

    private void AddDinosaurCandidates(
        List<ComparisonCandidate> candidates,
        List<ComparisonSourceStatus> statuses,
        IReadOnlyList<ComparisonFieldDefinition> fieldDefinitions)
    {
        try
        {
            IReadOnlyList<DinosaurProfile> profiles = _dinosaurCatalogService.GetProfiles();
            candidates.AddRange(profiles.Select(profile => CreateDinosaurCandidate(profile, fieldDefinitions)));
            statuses.Add(CreateAvailableStatus(ExplorationSourceType.Dinosaurs, profiles.Count));
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Comparison source {Source} could not be loaded.", ExplorationSourceType.Dinosaurs.ToCode());
            statuses.Add(CreateUnavailableStatus(ExplorationSourceType.Dinosaurs));
        }
    }

    private void AddAquariumCandidates(
        List<ComparisonCandidate> candidates,
        List<ComparisonSourceStatus> statuses,
        IReadOnlyList<ComparisonFieldDefinition> fieldDefinitions)
    {
        try
        {
            Dictionary<string, AquariumHabitatCategory> categoryByCode = _aquariumCatalogService
                .GetHabitatCategories()
                .ToDictionary(category => category.Code, StringComparer.Ordinal);
            IReadOnlyList<AquariumAnimalProfile> profiles = _aquariumCatalogService.GetProfiles();

            candidates.AddRange(profiles.Select(profile =>
                CreateAquariumCandidate(profile, categoryByCode[profile.HabitatCategory], fieldDefinitions)));
            statuses.Add(CreateAvailableStatus(ExplorationSourceType.Aquarium, profiles.Count));
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Comparison source {Source} could not be loaded.", ExplorationSourceType.Aquarium.ToCode());
            statuses.Add(CreateUnavailableStatus(ExplorationSourceType.Aquarium));
        }
    }

    private static ComparisonCandidate CreateDinosaurCandidate(
        DinosaurProfile profile,
        IReadOnlyList<ComparisonFieldDefinition> fieldDefinitions)
    {
        ExplorationSourceType source = ExplorationSourceType.Dinosaurs;
        string sourceLabelZhTW = source.GetLabel(LanguageCode.ZhTW);
        string sourceLabelEn = source.GetLabel(LanguageCode.En);
        string detailHref = $"/{source.GetRoutePrefix()}/{profile.Slug}";
        Dictionary<string, ComparisonFieldValue> fieldValues = CreateBaseFieldValues(
            sourceLabelZhTW,
            sourceLabelEn,
            profile.Names.Get(LanguageCode.ZhTW),
            profile.Names.Get(LanguageCode.En),
            profile.Diet.Get(LanguageCode.ZhTW),
            profile.Diet.Get(LanguageCode.En),
            profile.DiscoveryLocations.Get(LanguageCode.ZhTW),
            profile.DiscoveryLocations.Get(LanguageCode.En),
            profile.Summary.Get(LanguageCode.ZhTW),
            profile.Summary.Get(LanguageCode.En),
            detailHref);

        fieldValues["living-area"] = CreateNotApplicableValue(
            fieldDefinitions,
            "living-area",
            "恐龍朋友生活在很久以前，沒有水族館生活區域分類。",
            "Dinosaur friends lived long ago, so aquarium living areas do not fit.");
        fieldValues["period"] = CreateAvailableValue(
            profile.Periods.Get(LanguageCode.ZhTW),
            profile.Periods.Get(LanguageCode.En));

        return new ComparisonCandidate
        {
            StableId = $"{source.ToCode()}:{profile.Slug}",
            Source = source,
            Slug = profile.Slug,
            DetailHref = detailHref,
            SortOrder = profile.SortOrder,
            SourceSortOrder = source.GetSortOrder(),
            SourceLabelZhTW = sourceLabelZhTW,
            SourceLabelEn = sourceLabelEn,
            NameZhTW = profile.Names.Get(LanguageCode.ZhTW),
            NameEn = profile.Names.Get(LanguageCode.En),
            SummaryZhTW = profile.Summary.Get(LanguageCode.ZhTW),
            SummaryEn = profile.Summary.Get(LanguageCode.En),
            DietZhTW = profile.Diet.Get(LanguageCode.ZhTW),
            DietEn = profile.Diet.Get(LanguageCode.En),
            DiscoveryLocationsZhTW = profile.DiscoveryLocations.Get(LanguageCode.ZhTW),
            DiscoveryLocationsEn = profile.DiscoveryLocations.Get(LanguageCode.En),
            ImagePath = profile.MainImage.Path,
            ImageAltTextZhTW = profile.MainImage.AltText.Get(LanguageCode.ZhTW),
            ImageAltTextEn = profile.MainImage.AltText.Get(LanguageCode.En),
            FieldValues = fieldValues
        };
    }

    private static ComparisonCandidate CreateAquariumCandidate(
        AquariumAnimalProfile profile,
        AquariumHabitatCategory category,
        IReadOnlyList<ComparisonFieldDefinition> fieldDefinitions)
    {
        ExplorationSourceType source = ExplorationSourceType.Aquarium;
        string sourceLabelZhTW = source.GetLabel(LanguageCode.ZhTW);
        string sourceLabelEn = source.GetLabel(LanguageCode.En);
        string detailHref = $"/{source.GetRoutePrefix()}/{profile.Slug}";
        Dictionary<string, ComparisonFieldValue> fieldValues = CreateBaseFieldValues(
            sourceLabelZhTW,
            sourceLabelEn,
            profile.Names.Get(LanguageCode.ZhTW),
            profile.Names.Get(LanguageCode.En),
            profile.Diet.Get(LanguageCode.ZhTW),
            profile.Diet.Get(LanguageCode.En),
            profile.DiscoveryLocations.Get(LanguageCode.ZhTW),
            profile.DiscoveryLocations.Get(LanguageCode.En),
            profile.Summary.Get(LanguageCode.ZhTW),
            profile.Summary.Get(LanguageCode.En),
            detailHref);

        fieldValues["living-area"] = CreateAvailableValue(
            profile.Habitat.Get(LanguageCode.ZhTW),
            profile.Habitat.Get(LanguageCode.En));
        fieldValues["period"] = CreateNotApplicableValue(
            fieldDefinitions,
            "period",
            "水族館朋友生活在今天的水世界，不用恐龍時期分類。",
            "Aquarium friends live in today's water world, so dinosaur periods do not fit.");

        return new ComparisonCandidate
        {
            StableId = $"{source.ToCode()}:{profile.Slug}",
            Source = source,
            Slug = profile.Slug,
            DetailHref = detailHref,
            SortOrder = profile.SortOrder,
            SourceSortOrder = source.GetSortOrder(),
            SourceLabelZhTW = sourceLabelZhTW,
            SourceLabelEn = sourceLabelEn,
            NameZhTW = profile.Names.Get(LanguageCode.ZhTW),
            NameEn = profile.Names.Get(LanguageCode.En),
            SummaryZhTW = profile.Summary.Get(LanguageCode.ZhTW),
            SummaryEn = profile.Summary.Get(LanguageCode.En),
            DietZhTW = profile.Diet.Get(LanguageCode.ZhTW),
            DietEn = profile.Diet.Get(LanguageCode.En),
            DiscoveryLocationsZhTW = profile.DiscoveryLocations.Get(LanguageCode.ZhTW),
            DiscoveryLocationsEn = profile.DiscoveryLocations.Get(LanguageCode.En),
            ImagePath = profile.MainImage.Path,
            ImageAltTextZhTW = profile.MainImage.AltText.Get(LanguageCode.ZhTW),
            ImageAltTextEn = profile.MainImage.AltText.Get(LanguageCode.En),
            FieldValues = fieldValues
        };
    }

    private static Dictionary<string, ComparisonFieldValue> CreateBaseFieldValues(
        string sourceLabelZhTW,
        string sourceLabelEn,
        string nameZhTW,
        string nameEn,
        string dietZhTW,
        string dietEn,
        string discoveryLocationsZhTW,
        string discoveryLocationsEn,
        string summaryZhTW,
        string summaryEn,
        string detailHref)
    {
        return new Dictionary<string, ComparisonFieldValue>(StringComparer.Ordinal)
        {
            ["source"] = CreateAvailableValue(sourceLabelZhTW, sourceLabelEn),
            ["name"] = CreateAvailableValue(nameZhTW, nameEn),
            ["diet"] = CreateAvailableValue(dietZhTW, dietEn),
            ["discovery-location"] = CreateAvailableValue(discoveryLocationsZhTW, discoveryLocationsEn),
            ["summary"] = CreateAvailableValue(summaryZhTW, summaryEn),
            ["detail-link"] = new()
            {
                State = ComparisonFieldValueState.Available,
                TextZhTW = "閱讀故事",
                TextEn = "Read story",
                Href = detailHref
            }
        };
    }

    private static ComparisonFieldValue CreateAvailableValue(string textZhTW, string textEn)
    {
        return new ComparisonFieldValue
        {
            State = ComparisonFieldValueState.Available,
            TextZhTW = textZhTW,
            TextEn = textEn
        };
    }

    private static ComparisonFieldValue CreateNotApplicableValue(
        IReadOnlyList<ComparisonFieldDefinition> fieldDefinitions,
        string fieldCode,
        string textZhTW,
        string textEn)
    {
        ComparisonFieldDefinition? definition = fieldDefinitions.FirstOrDefault(field => field.Code == fieldCode);

        return new ComparisonFieldValue
        {
            State = ComparisonFieldValueState.NotApplicable,
            TextZhTW = string.IsNullOrWhiteSpace(textZhTW)
                ? definition?.GetNotApplicableText(LanguageCode.ZhTW) ?? string.Empty
                : textZhTW,
            TextEn = string.IsNullOrWhiteSpace(textEn)
                ? definition?.GetNotApplicableText(LanguageCode.En) ?? string.Empty
                : textEn
        };
    }

    private static ComparisonSourceStatus CreateAvailableStatus(ExplorationSourceType source, int count)
    {
        return new ComparisonSourceStatus
        {
            Source = source,
            IsAvailable = true,
            CandidateCount = count
        };
    }

    private static ComparisonSourceStatus CreateUnavailableStatus(ExplorationSourceType source)
    {
        return new ComparisonSourceStatus
        {
            Source = source,
            IsAvailable = false,
            CandidateCount = 0,
            FriendlyMessageZhTW = $"{source.GetLabel(LanguageCode.ZhTW)}故事朋友暫時躲起來了，其他朋友還可以先比較。",
            FriendlyMessageEn = $"{source.GetLabel(LanguageCode.En)} story friends are hiding for now, but other friends may still be ready."
        };
    }
}
