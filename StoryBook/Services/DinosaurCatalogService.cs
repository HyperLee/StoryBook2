using System.Globalization;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using StoryBook.Models;

namespace StoryBook.Services;

/// <summary>
/// Loads, validates, caches, looks up, and searches the local dinosaur catalog.
/// </summary>
public sealed class DinosaurCatalogService
{
    private readonly DinosaurCatalogOptions _options;
    private readonly IWebHostEnvironment _environment;
    private readonly DinosaurContentValidator _validator;
    private readonly ILogger<DinosaurCatalogService> _logger;
    private readonly Lazy<IReadOnlyList<DinosaurProfile>> _profiles;

    /// <summary>
    /// Creates a catalog service using options, environment paths, validation, and logging.
    /// </summary>
    public DinosaurCatalogService(
        IOptions<DinosaurCatalogOptions> options,
        IWebHostEnvironment environment,
        DinosaurContentValidator validator,
        ILogger<DinosaurCatalogService> logger)
    {
        _options = options.Value;
        _environment = environment;
        _validator = validator;
        _logger = logger;
        _profiles = new Lazy<IReadOnlyList<DinosaurProfile>>(LoadProfiles);
    }

    /// <summary>
    /// Gets all profiles sorted by catalog sort order. The parsed catalog is cached after first use.
    /// </summary>
    public IReadOnlyList<DinosaurProfile> GetProfiles()
    {
        return _profiles.Value;
    }

    /// <summary>
    /// Gets the first profile by sort order.
    /// </summary>
    public DinosaurProfile GetFirstProfile()
    {
        return GetProfiles().First();
    }

    /// <summary>
    /// Gets the previous profile in sort order, or <see langword="null" /> at the first item or unknown slug.
    /// </summary>
    public DinosaurProfile? GetPreviousProfile(string? slug)
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
    public DinosaurProfile? GetNextProfile(string? slug)
    {
        int index = FindProfileIndex(slug);
        IReadOnlyList<DinosaurProfile> profiles = GetProfiles();

        if (index < 0 || index >= profiles.Count - 1)
        {
            return null;
        }

        return profiles[index + 1];
    }

    /// <summary>
    /// Attempts to resolve a profile by slug and logs unknown slugs.
    /// </summary>
    public bool TryGetBySlug(string? slug, out DinosaurProfile? profile)
    {
        string normalizedSlug = slug?.Trim().ToLowerInvariant() ?? string.Empty;
        profile = GetProfiles().FirstOrDefault(item => item.Slug == normalizedSlug);

        if (profile is null)
        {
            _logger.LogWarning("Unknown dinosaur slug requested: {Slug}", slug);
            return false;
        }

        return true;
    }

    /// <summary>
    /// Searches local profile fields and keywords. Empty queries return all profiles in sort order.
    /// </summary>
    public IReadOnlyList<DinosaurProfile> Search(string? query, LanguageCode language)
    {
        string normalizedQuery = NormalizeForSearch(query);

        if (string.IsNullOrWhiteSpace(normalizedQuery))
        {
            return GetProfiles();
        }

        return GetProfiles()
            .Where(profile => BuildSearchText(profile, language).Contains(normalizedQuery, StringComparison.Ordinal))
            .ToList();
    }

    /// <summary>
    /// Maps sorted profiles into list-page search result projections for the selected language.
    /// </summary>
    public IReadOnlyList<DinosaurSearchResult> GetSearchResults(LanguageCode language)
    {
        return GetProfiles()
            .Select(profile => new DinosaurSearchResult
            {
                Slug = profile.Slug,
                Name = profile.Names.Get(language),
                NameZhTW = profile.Names.Get(LanguageCode.ZhTW),
                NameEn = profile.Names.Get(LanguageCode.En),
                Summary = profile.Summary.Get(language),
                SummaryZhTW = profile.Summary.Get(LanguageCode.ZhTW),
                SummaryEn = profile.Summary.Get(LanguageCode.En),
                ImagePath = profile.MainImage.Path,
                ImageAltText = profile.MainImage.AltText.Get(language),
                ImageAltTextZhTW = profile.MainImage.AltText.Get(LanguageCode.ZhTW),
                ImageAltTextEn = profile.MainImage.AltText.Get(LanguageCode.En),
                SearchText = BuildClientSearchText(profile),
                CategoryNote = profile.NotDinosaurNote?.Get(language),
                CategoryNoteZhTW = profile.NotDinosaurNote?.Get(LanguageCode.ZhTW),
                CategoryNoteEn = profile.NotDinosaurNote?.Get(LanguageCode.En)
            })
            .ToList();
    }

    private IReadOnlyList<DinosaurProfile> LoadProfiles()
    {
        string path = ResolveContentPath();

        try
        {
            string json = File.ReadAllText(path);
            DinosaurCatalog? catalog = JsonSerializer.Deserialize<DinosaurCatalog>(json, new JsonSerializerOptions(JsonSerializerDefaults.Web));
            DinosaurContentValidationResult result = _validator.ValidateCatalog(catalog);

            if (!result.IsValid)
            {
                foreach (string error in result.Errors)
                {
                    _logger.LogError("Dinosaur content validation error: {Error}", error);
                }

                throw new InvalidOperationException("Dinosaur content catalog failed validation.");
            }

            return catalog!.Profiles
                .OrderBy(profile => profile.SortOrder)
                .ToList();
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception exception) when (exception is IOException or JsonException or UnauthorizedAccessException)
        {
            _logger.LogError(exception, "Failed to load dinosaur content catalog from {Path}", path);
            throw;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unexpected error while loading dinosaur content catalog from {Path}", path);
            throw;
        }
    }

    private int FindProfileIndex(string? slug)
    {
        string normalizedSlug = slug?.Trim().ToLowerInvariant() ?? string.Empty;
        IReadOnlyList<DinosaurProfile> profiles = GetProfiles();

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

    private static string BuildSearchText(DinosaurProfile profile, LanguageCode language)
    {
        IEnumerable<string> fields =
        [
            profile.Names.Get(language),
            profile.Names.ZhTW,
            profile.Names.En,
            profile.Periods.Get(language),
            profile.Periods.ZhTW,
            profile.Periods.En,
            profile.Diet.Get(language),
            profile.Diet.ZhTW,
            profile.Diet.En,
            profile.DiscoveryLocations.Get(language),
            profile.DiscoveryLocations.ZhTW,
            profile.DiscoveryLocations.En,
            profile.SizeDescription.Get(language),
            profile.SizeDescription.ZhTW,
            profile.SizeDescription.En,
            profile.Summary.Get(language),
            profile.Summary.ZhTW,
            profile.Summary.En,
            profile.NotDinosaurNote?.Get(language) ?? string.Empty,
            profile.NotDinosaurNote?.ZhTW ?? string.Empty,
            profile.NotDinosaurNote?.En ?? string.Empty
        ];

        IEnumerable<string> keywords = profile.SearchKeywords.SelectMany(keyword => new[]
        {
            keyword.Get(language),
            keyword.ZhTW,
            keyword.En
        });

        return NormalizeForSearch(string.Join(' ', fields.Concat(keywords)));
    }

    private static string BuildClientSearchText(DinosaurProfile profile)
    {
        IEnumerable<string> fields =
        [
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
}
