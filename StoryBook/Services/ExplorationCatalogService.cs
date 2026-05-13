using System.Globalization;
using System.Text;
using StoryBook.Models;

namespace StoryBook.Services;

/// <summary>
/// Composes dinosaur and aquarium catalogs into the sitewide exploration projection.
/// </summary>
public sealed class ExplorationCatalogService
{
    private static readonly IReadOnlyDictionary<string, FacetGroupMetadata> FacetGroupMetadataByCode =
        new Dictionary<string, FacetGroupMetadata>(StringComparer.Ordinal)
        {
            ["source"] = new("source", "故事書來源", "Story source", 1),
            ["diet"] = new("diet", "食性", "Diet", 2),
            ["living-area"] = new("living-area", "生活區域", "Living area", 3),
            ["period"] = new("period", "生活時期", "Period", 4),
            ["discovery-location"] = new("discovery-location", "發現地點", "Found in", 5)
        };

    private readonly DinosaurCatalogService _dinosaurCatalogService;
    private readonly AquariumCatalogService _aquariumCatalogService;
    private readonly ILogger<ExplorationCatalogService> _logger;

    /// <summary>
    /// Creates the exploration projection service from existing storybook catalogs.
    /// </summary>
    public ExplorationCatalogService(
        DinosaurCatalogService dinosaurCatalogService,
        AquariumCatalogService aquariumCatalogService,
        ILogger<ExplorationCatalogService> logger)
    {
        _dinosaurCatalogService = dinosaurCatalogService;
        _aquariumCatalogService = aquariumCatalogService;
        _logger = logger;
    }

    /// <summary>
    /// Builds the current exploration snapshot with source statuses and facet groups.
    /// </summary>
    public ExplorationCatalogSnapshot GetSnapshot()
    {
        List<ExplorationItem> items = [];
        List<ExplorationSourceStatus> statuses = [];

        AddDinosaurItems(items, statuses);
        AddAquariumItems(items, statuses);

        IReadOnlyList<ExplorationItem> sortedItems = items
            .OrderBy(item => item.Source.GetSortOrder())
            .ThenBy(item => item.SortOrder)
            .ToList();

        return new ExplorationCatalogSnapshot(
            sortedItems,
            BuildFacetGroups(sortedItems),
            statuses.OrderBy(status => status.Source.GetSortOrder()).ToList());
    }

    private void AddDinosaurItems(List<ExplorationItem> items, List<ExplorationSourceStatus> statuses)
    {
        try
        {
            IReadOnlyList<DinosaurProfile> profiles = _dinosaurCatalogService.GetProfiles();
            items.AddRange(profiles.Select(CreateDinosaurItem));
            statuses.Add(CreateAvailableStatus(ExplorationSourceType.Dinosaurs, profiles.Count));
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Exploration source {Source} could not be loaded.", ExplorationSourceType.Dinosaurs.ToCode());
            statuses.Add(CreateUnavailableStatus(ExplorationSourceType.Dinosaurs));
        }
    }

    private void AddAquariumItems(List<ExplorationItem> items, List<ExplorationSourceStatus> statuses)
    {
        try
        {
            IReadOnlyList<AquariumHabitatCategory> categories = _aquariumCatalogService.GetHabitatCategories();
            Dictionary<string, AquariumHabitatCategory> categoryByCode = categories.ToDictionary(category => category.Code, StringComparer.Ordinal);
            IReadOnlyList<AquariumAnimalProfile> profiles = _aquariumCatalogService.GetProfiles();

            items.AddRange(profiles.Select(profile => CreateAquariumItem(profile, categoryByCode[profile.HabitatCategory])));
            statuses.Add(CreateAvailableStatus(ExplorationSourceType.Aquarium, profiles.Count));
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Exploration source {Source} could not be loaded.", ExplorationSourceType.Aquarium.ToCode());
            statuses.Add(CreateUnavailableStatus(ExplorationSourceType.Aquarium));
        }
    }

    private static ExplorationItem CreateDinosaurItem(DinosaurProfile profile)
    {
        ExplorationSourceType source = ExplorationSourceType.Dinosaurs;
        IReadOnlyList<ExplorationFacetValue> facets =
        [
            CreateSourceFacet(source),
            CreateTextFacet("diet", profile.Diet.ZhTW, profile.Diet.En, source),
            CreateTextFacet("period", profile.Periods.ZhTW, profile.Periods.En, source),
            CreateTextFacet("discovery-location", profile.DiscoveryLocations.ZhTW, profile.DiscoveryLocations.En, source)
        ];

        return new ExplorationItem
        {
            StableId = $"{source.ToCode()}:{profile.Slug}",
            Source = source,
            Slug = profile.Slug,
            DetailHref = $"/{source.GetRoutePrefix()}/{profile.Slug}",
            SortOrder = profile.SortOrder,
            SourceLabelZhTW = source.GetLabel(LanguageCode.ZhTW),
            SourceLabelEn = source.GetLabel(LanguageCode.En),
            NameZhTW = profile.Names.Get(LanguageCode.ZhTW),
            NameEn = profile.Names.Get(LanguageCode.En),
            SummaryZhTW = profile.Summary.Get(LanguageCode.ZhTW),
            SummaryEn = profile.Summary.Get(LanguageCode.En),
            ImagePath = profile.MainImage.Path,
            ImageAltTextZhTW = profile.MainImage.AltText.Get(LanguageCode.ZhTW),
            ImageAltTextEn = profile.MainImage.AltText.Get(LanguageCode.En),
            SearchText = BuildDinosaurSearchText(profile, source),
            Facets = facets
        };
    }

    private static ExplorationItem CreateAquariumItem(AquariumAnimalProfile profile, AquariumHabitatCategory category)
    {
        ExplorationSourceType source = ExplorationSourceType.Aquarium;
        IReadOnlyList<ExplorationFacetValue> facets =
        [
            CreateSourceFacet(source),
            CreateTextFacet("diet", profile.Diet.ZhTW, profile.Diet.En, source),
            new()
            {
                GroupCode = "living-area",
                ValueCode = category.Code,
                LabelZhTW = category.DisplayName.Get(LanguageCode.ZhTW),
                LabelEn = category.DisplayName.Get(LanguageCode.En),
                SourceScope = source,
                SortOrder = category.SortOrder
            },
            CreateTextFacet("discovery-location", profile.DiscoveryLocations.ZhTW, profile.DiscoveryLocations.En, source)
        ];

        return new ExplorationItem
        {
            StableId = $"{source.ToCode()}:{profile.Slug}",
            Source = source,
            Slug = profile.Slug,
            DetailHref = $"/{source.GetRoutePrefix()}/{profile.Slug}",
            SortOrder = profile.SortOrder,
            SourceLabelZhTW = source.GetLabel(LanguageCode.ZhTW),
            SourceLabelEn = source.GetLabel(LanguageCode.En),
            NameZhTW = profile.Names.Get(LanguageCode.ZhTW),
            NameEn = profile.Names.Get(LanguageCode.En),
            SummaryZhTW = profile.Summary.Get(LanguageCode.ZhTW),
            SummaryEn = profile.Summary.Get(LanguageCode.En),
            ImagePath = profile.MainImage.Path,
            ImageAltTextZhTW = profile.MainImage.AltText.Get(LanguageCode.ZhTW),
            ImageAltTextEn = profile.MainImage.AltText.Get(LanguageCode.En),
            SearchText = BuildAquariumSearchText(profile, category, source),
            Facets = facets
        };
    }

    private static ExplorationFacetValue CreateSourceFacet(ExplorationSourceType source)
    {
        return new ExplorationFacetValue
        {
            GroupCode = "source",
            ValueCode = source.ToCode(),
            LabelZhTW = source.GetLabel(LanguageCode.ZhTW),
            LabelEn = source.GetLabel(LanguageCode.En),
            SourceScope = source,
            SortOrder = source.GetSortOrder()
        };
    }

    private static ExplorationFacetValue CreateTextFacet(
        string groupCode,
        string labelZhTW,
        string labelEn,
        ExplorationSourceType source)
    {
        return new ExplorationFacetValue
        {
            GroupCode = groupCode,
            ValueCode = CreateValueCode(labelEn, labelZhTW),
            LabelZhTW = labelZhTW,
            LabelEn = labelEn,
            SourceScope = source,
            SortOrder = 1000
        };
    }

    private static ExplorationSourceStatus CreateAvailableStatus(ExplorationSourceType source, int itemCount)
    {
        return new ExplorationSourceStatus
        {
            Source = source,
            IsAvailable = true,
            ItemCount = itemCount
        };
    }

    private static ExplorationSourceStatus CreateUnavailableStatus(ExplorationSourceType source)
    {
        return new ExplorationSourceStatus
        {
            Source = source,
            IsAvailable = false,
            ItemCount = 0,
            FriendlyMessageZhTW = $"{source.GetLabel(LanguageCode.ZhTW)}故事暫時躲起來了，其他故事還可以先看看。",
            FriendlyMessageEn = $"{source.GetLabel(LanguageCode.En)} stories are hiding for now, but other stories may still be ready."
        };
    }

    private static IReadOnlyList<ExplorationFacetGroup> BuildFacetGroups(IReadOnlyList<ExplorationItem> items)
    {
        Dictionary<string, ExplorationFacetValue> valuesByKey = new(StringComparer.Ordinal);

        foreach (ExplorationFacetValue facet in items.SelectMany(item => item.Facets))
        {
            string key = $"{facet.GroupCode}:{facet.ValueCode}";
            valuesByKey.TryAdd(key, facet);
        }

        return FacetGroupMetadataByCode.Values
            .Select(metadata => new ExplorationFacetGroup
            {
                Code = metadata.Code,
                LabelZhTW = metadata.LabelZhTW,
                LabelEn = metadata.LabelEn,
                SortOrder = metadata.SortOrder,
                SelectionMode = "single",
                Values = valuesByKey.Values
                    .Where(value => value.GroupCode == metadata.Code)
                    .OrderBy(value => value.SortOrder)
                    .ThenBy(value => value.ValueCode, StringComparer.Ordinal)
                    .ToList()
            })
            .Where(group => group.Values.Count > 0)
            .OrderBy(group => group.SortOrder)
            .ToList();
    }

    private static string BuildDinosaurSearchText(DinosaurProfile profile, ExplorationSourceType source)
    {
        IEnumerable<string> fields =
        [
            source.GetLabel(LanguageCode.ZhTW),
            source.GetLabel(LanguageCode.En),
            profile.Names.ZhTW,
            profile.Names.En,
            profile.Periods.ZhTW,
            profile.Periods.En,
            profile.Diet.ZhTW,
            profile.Diet.En,
            profile.DiscoveryLocations.ZhTW,
            profile.DiscoveryLocations.En,
            profile.SizeDescription.ZhTW,
            profile.SizeDescription.En,
            profile.Summary.ZhTW,
            profile.Summary.En,
            profile.NotDinosaurNote?.ZhTW ?? string.Empty,
            profile.NotDinosaurNote?.En ?? string.Empty
        ];

        return JoinSearchText(fields, profile.SearchKeywords.SelectMany(keyword => new[] { keyword.ZhTW, keyword.En }));
    }

    private static string BuildAquariumSearchText(
        AquariumAnimalProfile profile,
        AquariumHabitatCategory category,
        ExplorationSourceType source)
    {
        IEnumerable<string> fields =
        [
            source.GetLabel(LanguageCode.ZhTW),
            source.GetLabel(LanguageCode.En),
            profile.Names.ZhTW,
            profile.Names.En,
            category.DisplayName.ZhTW,
            category.DisplayName.En,
            profile.Habitat.ZhTW,
            profile.Habitat.En,
            profile.Diet.ZhTW,
            profile.Diet.En,
            profile.DiscoveryLocations.ZhTW,
            profile.DiscoveryLocations.En,
            profile.Summary.ZhTW,
            profile.Summary.En
        ];

        return JoinSearchText(fields, profile.SearchKeywords.SelectMany(keyword => new[] { keyword.ZhTW, keyword.En }));
    }

    private static string JoinSearchText(IEnumerable<string> fields, IEnumerable<string> keywords)
    {
        return string.Join(' ', fields.Concat(keywords).Where(value => !string.IsNullOrWhiteSpace(value)));
    }

    private static string CreateValueCode(string preferred, string fallback)
    {
        string source = !string.IsNullOrWhiteSpace(preferred) ? preferred : fallback;
        string normalized = source.Normalize(NormalizationForm.FormKC).ToLower(CultureInfo.InvariantCulture);
        StringBuilder builder = new(normalized.Length);
        bool previousWasSeparator = false;

        foreach (char character in normalized)
        {
            if (char.IsLetterOrDigit(character))
            {
                builder.Append(character);
                previousWasSeparator = false;
            }
            else if (!previousWasSeparator && builder.Length > 0)
            {
                builder.Append('-');
                previousWasSeparator = true;
            }
        }

        return builder.ToString().Trim('-');
    }

    private sealed record FacetGroupMetadata(string Code, string LabelZhTW, string LabelEn, int SortOrder);
}

/// <summary>
/// Immutable snapshot of the exploration catalog projection.
/// </summary>
public sealed class ExplorationCatalogSnapshot
{
    /// <summary>
    /// Creates a projection snapshot.
    /// </summary>
    public ExplorationCatalogSnapshot(
        IReadOnlyList<ExplorationItem> items,
        IReadOnlyList<ExplorationFacetGroup> facetGroups,
        IReadOnlyList<ExplorationSourceStatus> sourceStatuses)
    {
        Items = items;
        FacetGroups = facetGroups;
        SourceStatuses = sourceStatuses;
    }

    /// <summary>
    /// Projected exploration items.
    /// </summary>
    public IReadOnlyList<ExplorationItem> Items { get; }

    /// <summary>
    /// Facet groups available for the projected items.
    /// </summary>
    public IReadOnlyList<ExplorationFacetGroup> FacetGroups { get; }

    /// <summary>
    /// Availability status for each source.
    /// </summary>
    public IReadOnlyList<ExplorationSourceStatus> SourceStatuses { get; }

    /// <summary>
    /// Whether at least one source returned items.
    /// </summary>
    public bool HasAvailableSource => Items.Count > 0;

    /// <summary>
    /// Whether at least one source failed while another source remained available.
    /// </summary>
    public bool HasPartialFailure => HasAvailableSource && SourceStatuses.Any(status => !status.IsAvailable);

    /// <summary>
    /// Whether all configured sources failed.
    /// </summary>
    public bool HasAllSourcesFailed => !HasAvailableSource && SourceStatuses.Any(status => !status.IsAvailable);
}
