using Microsoft.AspNetCore.Mvc.RazorPages;
using StoryBook.Models;
using StoryBook.Services;

namespace StoryBook.Pages.Passport;

public sealed class IndexModel : PageModel
{
    private readonly PassportCatalogService _catalogService;
    private readonly PassportPreferenceService _preferenceService;
    private readonly LanguagePreferenceService _languagePreferenceService;

    public IndexModel(
        PassportCatalogService catalogService,
        PassportPreferenceService preferenceService,
        LanguagePreferenceService languagePreferenceService)
    {
        _catalogService = catalogService;
        _preferenceService = preferenceService;
        _languagePreferenceService = languagePreferenceService;
    }

    public IReadOnlyList<PassportStoryItem> Stories { get; private set; } = [];

    public IReadOnlyList<PassportBadgeDefinition> Badges { get; private set; } = [];

    public IReadOnlyList<PassportSourceStatus> SourceStatuses { get; private set; } = [];

    public int TotalStoryCount { get; private set; }

    public bool HasAnyStory { get; private set; }

    public bool HasPartialSourceFailure { get; private set; }

    public bool HasAllSourcesFailed { get; private set; }

    public string StorageKey => _preferenceService.StorageKey;

    public int StateVersion => _preferenceService.StateVersion;

    public string LanguageStorageKey => _languagePreferenceService.StorageKey;

    public void OnGet()
    {
        ViewData["UsePassportAssets"] = true;

        PassportCatalogSnapshot snapshot = _catalogService.GetSnapshot();
        Stories = snapshot.Stories;
        Badges = snapshot.Badges;
        SourceStatuses = snapshot.SourceStatuses;
        TotalStoryCount = snapshot.TotalStoryCount;
        HasAnyStory = snapshot.HasAnyStory;
        HasPartialSourceFailure = snapshot.HasPartialSourceFailure;
        HasAllSourcesFailed = snapshot.HasAllSourcesFailed;
    }
}
