using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using StoryBook.Models;

namespace StoryBook.Services;

/// <summary>
/// Loads, validates, caches, and resolves learning journeys from existing storybook catalogs.
/// </summary>
public sealed class LearningJourneyCatalogService
{
    private readonly LearningJourneyCatalogOptions _options;
    private readonly IWebHostEnvironment _environment;
    private readonly LearningJourneyContentValidator _validator;
    private readonly DinosaurCatalogService _dinosaurCatalogService;
    private readonly AquariumCatalogService _aquariumCatalogService;
    private readonly ILogger<LearningJourneyCatalogService> _logger;
    private readonly Lazy<JourneyCatalog> _catalog;
    private readonly Lazy<JourneyCatalogSnapshot> _snapshot;

    /// <summary>
    /// Creates a learning journey catalog service from configuration, source catalogs, validation, and logging.
    /// </summary>
    public LearningJourneyCatalogService(
        IOptions<LearningJourneyCatalogOptions> options,
        IWebHostEnvironment environment,
        LearningJourneyContentValidator validator,
        DinosaurCatalogService dinosaurCatalogService,
        AquariumCatalogService aquariumCatalogService,
        ILogger<LearningJourneyCatalogService> logger)
    {
        _options = options.Value;
        _environment = environment;
        _validator = validator;
        _dinosaurCatalogService = dinosaurCatalogService;
        _aquariumCatalogService = aquariumCatalogService;
        _logger = logger;
        _catalog = new Lazy<JourneyCatalog>(LoadCatalog);
        _snapshot = new Lazy<JourneyCatalogSnapshot>(LoadSnapshot);
    }

    /// <summary>
    /// Builds the current learning journey snapshot.
    /// </summary>
    public JourneyCatalogSnapshot GetSnapshot()
    {
        return _snapshot.Value;
    }

    /// <summary>
    /// Attempts to resolve a journey definition by slug.
    /// </summary>
    public bool TryGetJourneyBySlug(string? slug, out LearningJourney? journey)
    {
        journey = null;

        if (string.IsNullOrWhiteSpace(slug))
        {
            return false;
        }

        string normalizedSlug = slug.Trim().ToLowerInvariant();
        journey = _catalog.Value.Journeys.FirstOrDefault(item => item.Slug == normalizedSlug);

        if (journey is null)
        {
            _logger.LogWarning("Unknown learning journey slug requested: {JourneySlug}", normalizedSlug);
            return false;
        }

        return true;
    }

    /// <summary>
    /// Gets resolved, ordered story items for a journey slug.
    /// </summary>
    public IReadOnlyList<JourneyStoryItem> GetStoryItems(string? journeySlug)
    {
        if (!TryGetJourneyBySlug(journeySlug, out LearningJourney? journey))
        {
            return [];
        }

        List<JourneyStoryItem> items = [];
        HashSet<string> stableIds = new(StringComparer.Ordinal);

        foreach (JourneyStoryReference reference in journey!.StoryReferences.OrderBy(reference => reference.SortOrder))
        {
            JourneyStoryItem? item = TryCreateStoryItem(reference);

            if (item is not null && stableIds.Add(item.StableId))
            {
                items.Add(item);
            }
        }

        return items
            .OrderBy(item => item.SortOrder)
            .ThenBy(item => item.StableId, StringComparer.Ordinal)
            .ToList();
    }

    /// <summary>
    /// Gets the first resolved story href for a journey slug, or <see langword="null" /> when none is available.
    /// </summary>
    public string? GetStartReadingHref(string? journeySlug)
    {
        return GetStoryItems(journeySlug).FirstOrDefault()?.DetailHref;
    }

    /// <summary>
    /// Gets availability status for a journey slug, including friendly unavailable states.
    /// </summary>
    public JourneyAvailabilityStatus GetAvailabilityStatus(string? journeySlug)
    {
        if (!TryGetJourneyBySlug(journeySlug, out LearningJourney? journey))
        {
            string normalizedSlug = journeySlug?.Trim().ToLowerInvariant() ?? string.Empty;
            return CreateNotFoundStatus(normalizedSlug);
        }

        return CreateAvailabilityStatus(journey!, _snapshot.Value.SourceStatuses);
    }

    private JourneyCatalogSnapshot LoadSnapshot()
    {
        JourneyCatalog catalog = _catalog.Value;
        IReadOnlyList<JourneySourceStatus> sourceStatuses = LoadSourceStatuses();
        IReadOnlyList<JourneyAvailabilityStatus> availabilityStatuses = catalog.Journeys
            .Select(journey => CreateAvailabilityStatus(journey, sourceStatuses))
            .ToList();
        HashSet<string> availableSlugs = availabilityStatuses
            .Where(status => status.CanAppearInList)
            .Select(status => status.JourneySlug)
            .ToHashSet(StringComparer.Ordinal);
        IReadOnlyList<LearningJourney> availableJourneys = catalog.Journeys
            .Where(journey => availableSlugs.Contains(journey.Slug))
            .OrderBy(journey => journey.SortOrder)
            .ThenBy(journey => journey.Slug, StringComparer.Ordinal)
            .ToList();

        return new JourneyCatalogSnapshot(
            availableJourneys,
            availabilityStatuses.Where(status => !status.CanAppearInList).ToList(),
            sourceStatuses);
    }

    private JourneyCatalog LoadCatalog()
    {
        string path = ResolveContentPath();

        try
        {
            string json = File.ReadAllText(path);
            JourneyCatalog? catalog = JsonSerializer.Deserialize<JourneyCatalog>(json, new JsonSerializerOptions(JsonSerializerDefaults.Web));
            LearningJourneyContentValidationResult result = _validator.ValidateCatalog(catalog);

            if (!result.IsValid)
            {
                foreach (string error in result.Errors)
                {
                    _logger.LogError("Learning journey content validation error: {ReasonCode}", error);
                }

                throw new InvalidOperationException("Learning journey content catalog failed validation.");
            }

            return catalog!;
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception exception) when (exception is IOException or JsonException or UnauthorizedAccessException)
        {
            _logger.LogError("Learning journey content catalog could not be loaded: {ReasonCode}", "catalog-load-failed");
            throw new InvalidOperationException("Learning journey content catalog could not be loaded.", exception);
        }
    }

    private IReadOnlyList<JourneySourceStatus> LoadSourceStatuses()
    {
        return
        [
            LoadSourceStatus(ExplorationSourceType.Dinosaurs),
            LoadSourceStatus(ExplorationSourceType.Aquarium)
        ];
    }

    private JourneySourceStatus LoadSourceStatus(ExplorationSourceType source)
    {
        try
        {
            int itemCount = source == ExplorationSourceType.Dinosaurs
                ? _dinosaurCatalogService.GetProfiles().Count
                : _aquariumCatalogService.GetProfiles().Count;

            return new JourneySourceStatus
            {
                Source = source,
                IsAvailable = true,
                ItemCount = itemCount
            };
        }
        catch (Exception)
        {
            string sourceCode = source.ToCode();
            _logger.LogWarning("Learning journey source unavailable: {SourceCode}", sourceCode);

            return new JourneySourceStatus
            {
                Source = source,
                IsAvailable = false,
                ItemCount = 0,
                FriendlyMessage = new JourneyText
                {
                    ZhTW = $"{source.GetLabel(LanguageCode.ZhTW)}故事暫時躲起來了，其他旅程還可以先看看。",
                    En = $"{source.GetLabel(LanguageCode.En)} stories are hiding for now, but other journeys may still be ready."
                },
                DiagnosticSummary = new JourneyDiagnosticSummary
                {
                    ReasonCode = "source-load-failed",
                    SourceCode = sourceCode
                }
            };
        }
    }

    private string ResolveContentPath()
    {
        if (Path.IsPathRooted(_options.ContentPath))
        {
            return _options.ContentPath;
        }

        return Path.Combine(_environment.ContentRootPath, _options.ContentPath);
    }

    private JourneyStoryItem? TryCreateStoryItem(JourneyStoryReference reference)
    {
        if (!TryParseSource(reference.Source, out ExplorationSourceType source))
        {
            return null;
        }

        try
        {
            return source == ExplorationSourceType.Dinosaurs
                ? TryCreateDinosaurStoryItem(reference)
                : TryCreateAquariumStoryItem(reference);
        }
        catch (Exception)
        {
            return null;
        }
    }

    private JourneyStoryItem? TryCreateDinosaurStoryItem(JourneyStoryReference reference)
    {
        DinosaurProfile? profile = _dinosaurCatalogService.GetProfiles()
            .FirstOrDefault(item => item.Slug == reference.Slug);

        if (profile is null)
        {
            return null;
        }

        ExplorationSourceType source = ExplorationSourceType.Dinosaurs;

        return new JourneyStoryItem
        {
            StableId = $"{source.ToCode()}:{profile.Slug}",
            Source = source,
            Slug = profile.Slug,
            DetailHref = $"/{source.GetRoutePrefix()}/{profile.Slug}",
            SortOrder = reference.SortOrder,
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

    private JourneyStoryItem? TryCreateAquariumStoryItem(JourneyStoryReference reference)
    {
        AquariumAnimalProfile? profile = _aquariumCatalogService.GetProfiles()
            .FirstOrDefault(item => item.Slug == reference.Slug);

        if (profile is null)
        {
            return null;
        }

        ExplorationSourceType source = ExplorationSourceType.Aquarium;

        return new JourneyStoryItem
        {
            StableId = $"{source.ToCode()}:{profile.Slug}",
            Source = source,
            Slug = profile.Slug,
            DetailHref = $"/{source.GetRoutePrefix()}/{profile.Slug}",
            SortOrder = reference.SortOrder,
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

    private static bool TryParseSource(string? sourceCode, out ExplorationSourceType source)
    {
        source = ExplorationSourceType.Dinosaurs;

        return sourceCode?.Trim().ToLowerInvariant() switch
        {
            "dinosaurs" => true,
            "aquarium" => SetSource(ExplorationSourceType.Aquarium, out source),
            _ => false
        };
    }

    private static bool SetSource(ExplorationSourceType value, out ExplorationSourceType source)
    {
        source = value;
        return true;
    }

    private JourneyAvailabilityStatus CreateAvailabilityStatus(
        LearningJourney journey,
        IReadOnlyList<JourneySourceStatus> sourceStatuses)
    {
        IReadOnlyList<JourneyStoryItem> storyItems = GetStoryItems(journey.Slug);
        JourneyAvailabilityState state = GetAvailabilityState(journey, storyItems, sourceStatuses);
        bool canAppearInList = state == JourneyAvailabilityState.Available;
        JourneyDiagnosticSummary? diagnostic = canAppearInList
            ? null
            : new JourneyDiagnosticSummary
            {
                ReasonCode = ToReasonCode(state),
                JourneySlug = journey.Slug,
                Count = storyItems.Count
            };

        if (!canAppearInList)
        {
            _logger.LogWarning(
                "Learning journey unavailable: {JourneySlug} {ReasonCode} {Count}",
                journey.Slug,
                diagnostic?.ReasonCode,
                storyItems.Count);
        }

        return new JourneyAvailabilityStatus
        {
            JourneySlug = journey.Slug,
            State = state,
            ValidItemCount = storyItems.Count,
            CanAppearInList = canAppearInList,
            FriendlyMessage = CreateFriendlyMessage(state),
            DiagnosticSummary = diagnostic
        };
    }

    private static JourneyAvailabilityState GetAvailabilityState(
        LearningJourney journey,
        IReadOnlyList<JourneyStoryItem> storyItems,
        IReadOnlyList<JourneySourceStatus> sourceStatuses)
    {
        if (HasMissingRequiredText(journey))
        {
            return JourneyAvailabilityState.MissingRequiredText;
        }

        bool referencesUnavailableSource = journey.StoryReferences.Any(reference =>
            TryParseSource(reference.Source, out ExplorationSourceType source)
            && sourceStatuses.Any(status => status.Source == source && !status.IsAvailable));

        if (referencesUnavailableSource && storyItems.Count < 3)
        {
            return JourneyAvailabilityState.SourceUnavailable;
        }

        if (storyItems.Count < 3)
        {
            return JourneyAvailabilityState.NotEnoughItems;
        }

        if (storyItems.Count > 5)
        {
            return JourneyAvailabilityState.TooManyItems;
        }

        return JourneyAvailabilityState.Available;
    }

    private static bool HasMissingRequiredText(LearningJourney journey)
    {
        return string.IsNullOrWhiteSpace(journey.Slug)
            || string.IsNullOrWhiteSpace(journey.Title.Get(LanguageCode.ZhTW))
            || string.IsNullOrWhiteSpace(journey.Summary.Get(LanguageCode.ZhTW))
            || journey.LearningGoals.Count is < 1 or > 3
            || journey.LearningGoals.Any(goal => string.IsNullOrWhiteSpace(goal.Get(LanguageCode.ZhTW)))
            || journey.SuggestedReadingMinutes < 1
            || string.IsNullOrWhiteSpace(journey.AgeGuidance.Get(LanguageCode.ZhTW));
    }

    private static JourneyAvailabilityStatus CreateNotFoundStatus(string journeySlug)
    {
        return new JourneyAvailabilityStatus
        {
            JourneySlug = journeySlug,
            State = JourneyAvailabilityState.NotFound,
            ValidItemCount = 0,
            CanAppearInList = false,
            FriendlyMessage = new JourneyText
            {
                ZhTW = "找不到這條旅程，先回旅程列表看看其他路線。",
                En = "This journey cannot be found. Return to the journey list and try another route."
            },
            DiagnosticSummary = new JourneyDiagnosticSummary
            {
                ReasonCode = "not-found",
                JourneySlug = journeySlug
            }
        };
    }

    private static JourneyText CreateFriendlyMessage(JourneyAvailabilityState state)
    {
        return state switch
        {
            JourneyAvailabilityState.NotEnoughItems => new JourneyText
            {
                ZhTW = "這條旅程暫時不能出發，故事朋友還沒有排滿三位。",
                En = "This journey is not ready yet because it needs at least three story friends."
            },
            JourneyAvailabilityState.TooManyItems => new JourneyText
            {
                ZhTW = "這條旅程暫時太長了，需要把故事範圍縮小一點。",
                En = "This journey is too long for now and needs a smaller story range."
            },
            JourneyAvailabilityState.SourceUnavailable => new JourneyText
            {
                ZhTW = "有些故事朋友暫時躲起來了，這條旅程暫時不能出發。",
                En = "Some story friends are hiding for now, so this journey is not ready yet."
            },
            JourneyAvailabilityState.MissingRequiredText => new JourneyText
            {
                ZhTW = "這條旅程還少一點說明文字，整理好就能出發。",
                En = "This journey still needs a little more text before it is ready."
            },
            JourneyAvailabilityState.InvalidReference => new JourneyText
            {
                ZhTW = "這條旅程有故事路標需要整理，暫時不能出發。",
                En = "This journey has story signs that need fixing before it is ready."
            },
            _ => new JourneyText()
        };
    }

    private static string ToReasonCode(JourneyAvailabilityState state)
    {
        return state switch
        {
            JourneyAvailabilityState.NotFound => "not-found",
            JourneyAvailabilityState.NotEnoughItems => "not-enough-items",
            JourneyAvailabilityState.TooManyItems => "too-many-items",
            JourneyAvailabilityState.MissingRequiredText => "missing-required-text",
            JourneyAvailabilityState.SourceUnavailable => "source-unavailable",
            JourneyAvailabilityState.InvalidReference => "invalid-reference",
            _ => "available"
        };
    }
}
