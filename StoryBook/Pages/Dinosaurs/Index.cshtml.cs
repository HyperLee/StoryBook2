using Microsoft.AspNetCore.Mvc.RazorPages;
using StoryBook.Models;
using StoryBook.Services;

namespace StoryBook.Pages.Dinosaurs;

public sealed class IndexModel : PageModel
{
    private readonly DinosaurCatalogService _catalogService;

    public IndexModel(DinosaurCatalogService catalogService)
    {
        _catalogService = catalogService;
    }

    public IReadOnlyList<DinosaurProfile> Profiles { get; private set; } = [];

    public IReadOnlyList<DinosaurSearchResult> SearchResults { get; private set; } = [];

    public DinosaurProfile? FirstProfile { get; private set; }

    public void OnGet()
    {
        ViewData["UseDinosaurAssets"] = true;
        Profiles = _catalogService.GetProfiles();
        SearchResults = _catalogService.GetSearchResults(LanguageCode.ZhTW);
        FirstProfile = Profiles.FirstOrDefault();
    }
}
