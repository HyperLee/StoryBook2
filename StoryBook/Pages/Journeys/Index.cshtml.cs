using Microsoft.AspNetCore.Mvc.RazorPages;
using StoryBook.Models;
using StoryBook.Services;

namespace StoryBook.Pages.Journeys;

public sealed class IndexModel : PageModel
{
    private readonly LearningJourneyCatalogService _catalogService;

    public IndexModel(LearningJourneyCatalogService catalogService)
    {
        _catalogService = catalogService;
    }

    public IReadOnlyList<LearningJourney> Journeys { get; private set; } = [];

    public IReadOnlyList<JourneySourceStatus> SourceStatuses { get; private set; } = [];

    public bool HasAnyAvailableJourney { get; private set; }

    public bool HasPartialSourceFailure { get; private set; }

    public bool HasAllSourcesFailed { get; private set; }

    public void OnGet()
    {
        ViewData["UseJourneyAssets"] = true;

        JourneyCatalogSnapshot snapshot = _catalogService.GetSnapshot();
        Journeys = snapshot.AvailableJourneys;
        SourceStatuses = snapshot.SourceStatuses;
        HasAnyAvailableJourney = snapshot.HasAnyAvailableJourney;
        HasPartialSourceFailure = snapshot.HasPartialSourceFailure;
        HasAllSourcesFailed = snapshot.HasAllSourcesFailed;
    }
}
