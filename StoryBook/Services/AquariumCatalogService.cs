using System.Globalization;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using StoryBook.Models;

namespace StoryBook.Services;

/// <summary>
/// Loads, validates, caches, looks up, and searches the local aquarium catalog.
/// </summary>
public sealed class AquariumCatalogService
{
    private readonly AquariumCatalogOptions _options;
    private readonly IWebHostEnvironment _environment;
    private readonly AquariumContentValidator _validator;
    private readonly ILogger<AquariumCatalogService> _logger;
    private readonly Lazy<AquariumCatalogSnapshot> _snapshot;

    /// <summary>
    /// Creates a catalog service using options, environment paths, validation, and logging.
    /// </summary>
    public AquariumCatalogService(
        IOptions<AquariumCatalogOptions> options,
        IWebHostEnvironment environment,
        AquariumContentValidator validator,
        ILogger<AquariumCatalogService> logger)
    {
        _options = options.Value;
        _environment = environment;
        _validator = validator;
        _logger = logger;
        _snapshot = new Lazy<AquariumCatalogSnapshot>(LoadCatalog);
    }

    /// <summary>
    /// Gets all profiles sorted by catalog sort order. The parsed catalog is cached after first use.
    /// </summary>
    public IReadOnlyList<AquariumAnimalProfile> GetProfiles()
    {
        return _snapshot.Value.Profiles;
    }

    /// <summary>
    /// Gets all habitat categories sorted by category sort order.
    /// </summary>
    public IReadOnlyList<AquariumHabitatCategory> GetHabitatCategories()
    {
        return _snapshot.Value.HabitatCategories;
    }

    /// <summary>
    /// Gets the first profile by sort order.
    /// </summary>
    public AquariumAnimalProfile GetFirstProfile()
    {
        return GetProfiles().First();
    }

    /// <summary>
    /// Gets the previous profile in sort order, or <see langword="null" /> at the first item or unknown slug.
    /// </summary>
    public AquariumAnimalProfile? GetPreviousProfile(string? slug)
    {
        int index = FindProfileIndex(slug);

        if (index <= 0)
        {
            return null;
        }

        return GetProfiles()[index - 1];
    }

    /// <summary>
    /// Gets the next profile in sort order, or <see langword="null" /> at the last item or unknown slug.
    /// </summary>
    public AquariumAnimalProfile? GetNextProfile(string? slug)
    {
        int index = FindProfileIndex(slug);
        IReadOnlyList<AquariumAnimalProfile> profiles = GetProfiles();

        if (index < 0 || index >= profiles.Count - 1)
        {
            return null;
        }

        return profiles[index + 1];
    }

    /// <summary>
    /// Attempts to resolve a profile by slug and logs unknown slugs.
    /// </summary>
    public bool TryGetBySlug(string? slug, out AquariumAnimalProfile? profile)
    {
        string normalizedSlug = slug?.Trim().ToLowerInvariant() ?? string.Empty;
        profile = GetProfiles().FirstOrDefault(item => item.Slug == normalizedSlug);

        if (profile is null)
        {
            _logger.LogWarning("Unknown aquarium slug requested: {Slug}", slug);
            return false;
        }

        return true;
    }

    /// <summary>
    /// Searches local profile fields and keywords. Empty queries return all profiles in sort order.
    /// </summary>
    public IReadOnlyList<AquariumAnimalProfile> Search(string? query, LanguageCode language)
    {
        string normalizedQuery = NormalizeForSearch(query);

        if (string.IsNullOrWhiteSpace(normalizedQuery) && string.IsNullOrWhiteSpace(query))
        {
            return GetProfiles();
        }

        if (normalizedQuery.Length < 2)
        {
            return [];
        }

        return GetProfiles()
            .Where(profile => BuildSearchText(profile, language).Contains(normalizedQuery, StringComparison.Ordinal))
            .ToList();
    }

    /// <summary>
    /// Gets whether a query is too short after search normalization.
    /// </summary>
    public bool IsQueryTooShort(string? query)
    {
        string normalizedQuery = NormalizeForSearch(query);
        return !string.IsNullOrWhiteSpace(query) && normalizedQuery.Length < 2;
    }

    /// <summary>
    /// Maps sorted profiles into list-page search result projections for the selected language.
    /// </summary>
    public IReadOnlyList<AquariumSearchResult> GetSearchResults(LanguageCode language)
    {
        Dictionary<string, AquariumHabitatCategory> categories = GetHabitatCategories()
            .ToDictionary(category => category.Code, StringComparer.Ordinal);

        return GetProfiles()
            .Select(profile =>
            {
                AquariumHabitatCategory category = categories[profile.HabitatCategory];

                return new AquariumSearchResult
                {
                    Slug = profile.Slug,
                    Name = profile.Names.Get(language),
                    NameZhTW = profile.Names.Get(LanguageCode.ZhTW),
                    NameEn = profile.Names.Get(LanguageCode.En),
                    HabitatCategoryName = category.DisplayName.Get(language),
                    HabitatCategoryNameZhTW = category.DisplayName.Get(LanguageCode.ZhTW),
                    HabitatCategoryNameEn = category.DisplayName.Get(LanguageCode.En),
                    Summary = profile.Summary.Get(language),
                    SummaryZhTW = profile.Summary.Get(LanguageCode.ZhTW),
                    SummaryEn = profile.Summary.Get(LanguageCode.En),
                    ImagePath = profile.MainImage.Path,
                    ImageAltText = profile.MainImage.AltText.Get(language),
                    ImageAltTextZhTW = profile.MainImage.AltText.Get(LanguageCode.ZhTW),
                    ImageAltTextEn = profile.MainImage.AltText.Get(LanguageCode.En),
                    SearchText = BuildClientSearchText(profile, category)
                };
            })
            .ToList();
    }

    private AquariumCatalogSnapshot LoadCatalog()
    {
        string path = ResolveContentPath();

        try
        {
            string json = File.ReadAllText(path);
            AquariumCatalog? catalog = JsonSerializer.Deserialize<AquariumCatalog>(json, new JsonSerializerOptions(JsonSerializerDefaults.Web));
            AquariumContentValidationResult result = _validator.ValidateCatalog(catalog);

            if (!result.IsValid)
            {
                foreach (string error in result.Errors)
                {
                    _logger.LogError("Aquarium content validation error: {Error}", error);
                }

                throw new InvalidOperationException("Aquarium content catalog failed validation.");
            }

            return new AquariumCatalogSnapshot(
                catalog!.HabitatCategories.OrderBy(category => category.SortOrder).ToList(),
                catalog.Animals.OrderBy(profile => profile.SortOrder).ToList());
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception exception) when (exception is IOException or JsonException or UnauthorizedAccessException)
        {
            _logger.LogError(exception, "Failed to load aquarium content catalog from {Path}", path);
            throw new InvalidOperationException("Aquarium content catalog could not be loaded.", exception);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unexpected error while loading aquarium content catalog from {Path}", path);
            throw;
        }
    }

    private int FindProfileIndex(string? slug)
    {
        string normalizedSlug = slug?.Trim().ToLowerInvariant() ?? string.Empty;
        IReadOnlyList<AquariumAnimalProfile> profiles = GetProfiles();

        for (int index = 0; index < profiles.Count; index++)
        {
            if (profiles[index].Slug == normalizedSlug)
            {
                return index;
            }
        }

        return -1;
    }

    private string ResolveContentPath()
    {
        if (Path.IsPathRooted(_options.ContentPath))
        {
            return _options.ContentPath;
        }

        return Path.Combine(_environment.ContentRootPath, _options.ContentPath);
    }

    private string BuildSearchText(AquariumAnimalProfile profile, LanguageCode language)
    {
        AquariumHabitatCategory category = GetHabitatCategories().First(item => item.Code == profile.HabitatCategory);
        IEnumerable<string> fields =
        [
            profile.Names.Get(language),
            profile.Names.ZhTW,
            profile.Names.En,
            category.DisplayName.Get(language),
            category.DisplayName.ZhTW,
            category.DisplayName.En,
            profile.Habitat.Get(language),
            profile.Habitat.ZhTW,
            profile.Habitat.En,
            profile.Diet.Get(language),
            profile.Diet.ZhTW,
            profile.Diet.En,
            profile.DiscoveryLocations.Get(language),
            profile.DiscoveryLocations.ZhTW,
            profile.DiscoveryLocations.En,
            profile.Summary.Get(language),
            profile.Summary.ZhTW,
            profile.Summary.En
        ];

        IEnumerable<string> keywords = profile.SearchKeywords.SelectMany(keyword => new[]
        {
            keyword.Get(language),
            keyword.ZhTW,
            keyword.En
        });

        return NormalizeForSearch(string.Join(' ', fields.Concat(keywords)));
    }

    private static string BuildClientSearchText(AquariumAnimalProfile profile, AquariumHabitatCategory category)
    {
        IEnumerable<string> fields =
        [
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

        IEnumerable<string> keywords = profile.SearchKeywords.SelectMany(keyword => new[]
        {
            keyword.ZhTW,
            keyword.En
        });

        return string.Join(' ', fields.Concat(keywords));
    }

    private static string NormalizeForSearch(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        string normalized = value.Normalize(NormalizationForm.FormKC).ToLower(CultureInfo.InvariantCulture);
        StringBuilder builder = new(normalized.Length);

        foreach (char character in normalized)
        {
            if (char.IsLetterOrDigit(character))
            {
                builder.Append(character);
            }
        }

        return builder.ToString();
    }

    private sealed record AquariumCatalogSnapshot(
        IReadOnlyList<AquariumHabitatCategory> HabitatCategories,
        IReadOnlyList<AquariumAnimalProfile> Profiles);
}
