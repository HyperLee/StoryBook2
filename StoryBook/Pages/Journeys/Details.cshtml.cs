using Microsoft.AspNetCore.Mvc.RazorPages;
using StoryBook.Models;
using StoryBook.Services;

namespace StoryBook.Pages.Journeys;

public sealed class DetailsModel : PageModel
{
    private readonly LearningJourneyCatalogService _catalogService;

    public DetailsModel(LearningJourneyCatalogService catalogService)
    {
        _catalogService = catalogService;
    }

    public LearningJourney? Journey { get; private set; }

    public IReadOnlyList<JourneyStoryItem> StoryItems { get; private set; } = [];

    public string? StartReadingHref { get; private set; }

    public bool HasAvailableJourney => Journey is not null && StoryItems.Count > 0 && StartReadingHref is not null;

    public void OnGet(string? slug)
    {
        ViewData["UseJourneyAssets"] = true;

        if (!_catalogService.TryGetJourneyBySlug(slug, out LearningJourney? journey))
        {
            return;
        }

        Journey = journey;
        StoryItems = _catalogService.GetStoryItems(journey!.Slug);
        StartReadingHref = _catalogService.GetStartReadingHref(journey.Slug);
    }
}
