using Microsoft.AspNetCore.Mvc.RazorPages;
using StoryBook.Models;
using StoryBook.Services;

namespace StoryBook.Pages.Explore;

public sealed class IndexModel : PageModel
{
    private readonly ExplorationCatalogService _catalogService;

    public IndexModel(ExplorationCatalogService catalogService)
    {
        _catalogService = catalogService;
    }

    public IReadOnlyList<ExplorationItem> Items { get; private set; } = [];

    public IReadOnlyList<ExplorationFacetGroup> FacetGroups { get; private set; } = [];

    public IReadOnlyList<ExplorationSourceStatus> SourceStatuses { get; private set; } = [];

    public bool HasPartialFailure { get; private set; }

    public bool HasAllSourcesFailed { get; private set; }

    public void OnGet()
    {
        ViewData["UseExploreAssets"] = true;

        ExplorationCatalogSnapshot snapshot = _catalogService.GetSnapshot();
        Items = snapshot.Items;
        FacetGroups = snapshot.FacetGroups;
        SourceStatuses = snapshot.SourceStatuses;
        HasPartialFailure = snapshot.HasPartialFailure;
        HasAllSourcesFailed = snapshot.HasAllSourcesFailed;
    }
}
