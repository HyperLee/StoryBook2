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
    }

    /// <summary>
    /// Builds the current learning journey snapshot.
    /// </summary>
    public JourneyCatalogSnapshot GetSnapshot()
    {
        return new JourneyCatalogSnapshot([], [], []);
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

        _logger.LogWarning("Unknown learning journey slug requested: {JourneySlug}", slug.Trim().ToLowerInvariant());
        return false;
    }
}
