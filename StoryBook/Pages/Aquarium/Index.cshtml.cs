using Microsoft.AspNetCore.Mvc.RazorPages;
using StoryBook.Models;
using StoryBook.Services;

namespace StoryBook.Pages.Aquarium;

public sealed class IndexModel : PageModel
{
    private readonly AquariumCatalogService _catalogService;

    public IndexModel(AquariumCatalogService catalogService)
    {
        _catalogService = catalogService;
    }

    public IReadOnlyList<AquariumAnimalProfile> Profiles { get; private set; } = [];

    public IReadOnlyList<AquariumSearchResult> SearchResults { get; private set; } = [];

    public AquariumAnimalProfile? FirstProfile { get; private set; }

    public bool HasLoadFailure { get; private set; }

    public void OnGet()
    {
        ViewData["UseAquariumAssets"] = true;

        try
        {
            Profiles = _catalogService.GetProfiles();
            SearchResults = _catalogService.GetSearchResults(LanguageCode.ZhTW);
            FirstProfile = Profiles.FirstOrDefault();
        }
        catch (InvalidOperationException)
        {
            HasLoadFailure = true;
        }
    }
}
