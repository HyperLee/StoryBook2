using StoryBook.Models;

namespace StoryBook.Services;

/// <summary>
/// Composes dinosaur and aquarium catalogs into the reading passport projection.
/// </summary>
public sealed class PassportCatalogService
{
    private readonly DinosaurCatalogService _dinosaurCatalogService;
    private readonly AquariumCatalogService _aquariumCatalogService;
    private readonly PassportPreferenceService _preferenceService;
    private readonly ILogger<PassportCatalogService> _logger;

    /// <summary>
    /// Creates a passport catalog service from existing storybook catalogs and passport metadata.
    /// </summary>
    public PassportCatalogService(
        DinosaurCatalogService dinosaurCatalogService,
        AquariumCatalogService aquariumCatalogService,
        PassportPreferenceService preferenceService,
        ILogger<PassportCatalogService> logger)
    {
        _dinosaurCatalogService = dinosaurCatalogService;
        _aquariumCatalogService = aquariumCatalogService;
        _preferenceService = preferenceService;
        _logger = logger;
    }

    /// <summary>
    /// Builds the current reading passport snapshot.
    /// </summary>
    public PassportCatalogSnapshot GetSnapshot()
    {
        List<PassportStoryItem> stories = [];
        List<PassportSourceStatus> sourceStatuses = [];

        AddDinosaurStories(stories, sourceStatuses);
        AddAquariumStories(stories, sourceStatuses);

        IReadOnlyList<PassportStoryItem> sortedStories = stories
            .OrderBy(story => story.SourceSortOrder)
            .ThenBy(story => story.SortOrder)
            .ThenBy(story => story.StableId, StringComparer.Ordinal)
            .ToList();

        return new PassportCatalogSnapshot(
            sortedStories,
            _preferenceService.Badges.OrderBy(badge => badge.SortOrder).ToList(),
            sourceStatuses.OrderBy(status => status.Source.GetSortOrder()).ToList());
    }

    private void AddDinosaurStories(List<PassportStoryItem> stories, List<PassportSourceStatus> sourceStatuses)
    {
        ExplorationSourceType source = ExplorationSourceType.Dinosaurs;

        try
        {
            IReadOnlyList<DinosaurProfile> profiles = _dinosaurCatalogService.GetProfiles();
            stories.AddRange(profiles.Select(CreateDinosaurStory));
            sourceStatuses.Add(CreateAvailableStatus(source, profiles.Count));
        }
        catch (Exception exception) when (exception is IOException or InvalidOperationException or UnauthorizedAccessException)
        {
            LogSourceFailure(source);
            sourceStatuses.Add(CreateUnavailableStatus(source));
        }
    }

    private void AddAquariumStories(List<PassportStoryItem> stories, List<PassportSourceStatus> sourceStatuses)
    {
        ExplorationSourceType source = ExplorationSourceType.Aquarium;

        try
        {
            IReadOnlyList<AquariumAnimalProfile> profiles = _aquariumCatalogService.GetProfiles();
            stories.AddRange(profiles.Select(CreateAquariumStory));
            sourceStatuses.Add(CreateAvailableStatus(source, profiles.Count));
        }
        catch (Exception exception) when (exception is IOException or InvalidOperationException or UnauthorizedAccessException)
        {
            LogSourceFailure(source);
            sourceStatuses.Add(CreateUnavailableStatus(source));
        }
    }

    private void LogSourceFailure(ExplorationSourceType source)
    {
        _logger.LogWarning(
            "Passport source {Source} could not be loaded. Reason: {ReasonCode}.",
            source.ToCode(),
            "source-load-failed");
    }

    private static PassportStoryItem CreateDinosaurStory(DinosaurProfile profile)
    {
        ExplorationSourceType source = ExplorationSourceType.Dinosaurs;
        string sourceCode = source.ToCode();

        return new PassportStoryItem
        {
            StableId = $"{sourceCode}:{profile.Slug}",
            Source = source,
            SourceCode = sourceCode,
            Slug = profile.Slug,
            SortOrder = profile.SortOrder,
            SourceSortOrder = source.GetSortOrder(),
            DetailHref = $"/{source.GetRoutePrefix()}/{profile.Slug}",
            SourceLabelZhTW = source.GetLabel(LanguageCode.ZhTW),
            SourceLabelEn = source.GetLabel(LanguageCode.En),
            NameZhTW = profile.Names.Get(LanguageCode.ZhTW),
            NameEn = profile.Names.Get(LanguageCode.En),
            SummaryZhTW = profile.Summary.Get(LanguageCode.ZhTW),
            SummaryEn = profile.Summary.Get(LanguageCode.En),
            ImagePath = profile.MainImage.Path,
            ImageAltTextZhTW = profile.MainImage.AltText.Get(LanguageCode.ZhTW),
            ImageAltTextEn = profile.MainImage.AltText.Get(LanguageCode.En)
        };
    }

    private static PassportStoryItem CreateAquariumStory(AquariumAnimalProfile profile)
    {
        ExplorationSourceType source = ExplorationSourceType.Aquarium;
        string sourceCode = source.ToCode();

        return new PassportStoryItem
        {
            StableId = $"{sourceCode}:{profile.Slug}",
            Source = source,
            SourceCode = sourceCode,
            Slug = profile.Slug,
            SortOrder = profile.SortOrder,
            SourceSortOrder = source.GetSortOrder(),
            DetailHref = $"/{source.GetRoutePrefix()}/{profile.Slug}",
            SourceLabelZhTW = source.GetLabel(LanguageCode.ZhTW),
            SourceLabelEn = source.GetLabel(LanguageCode.En),
            NameZhTW = profile.Names.Get(LanguageCode.ZhTW),
            NameEn = profile.Names.Get(LanguageCode.En),
            SummaryZhTW = profile.Summary.Get(LanguageCode.ZhTW),
            SummaryEn = profile.Summary.Get(LanguageCode.En),
            ImagePath = profile.MainImage.Path,
            ImageAltTextZhTW = profile.MainImage.AltText.Get(LanguageCode.ZhTW),
            ImageAltTextEn = profile.MainImage.AltText.Get(LanguageCode.En)
        };
    }

    private static PassportSourceStatus CreateAvailableStatus(ExplorationSourceType source, int storyCount)
    {
        return new PassportSourceStatus
        {
            Source = source,
            SourceCode = source.ToCode(),
            IsAvailable = true,
            StoryCount = storyCount
        };
    }

    private static PassportSourceStatus CreateUnavailableStatus(ExplorationSourceType source)
    {
        return new PassportSourceStatus
        {
            Source = source,
            SourceCode = source.ToCode(),
            IsAvailable = false,
            StoryCount = 0,
            ReasonCode = "source-load-failed",
            FriendlyMessageZhTW = $"{source.GetLabel(LanguageCode.ZhTW)}故事朋友暫時躲起來了，其他朋友還可以先探索。",
            FriendlyMessageEn = $"{source.GetLabel(LanguageCode.En)} story friends are hiding for now, but other friends may still be ready."
        };
    }
}
