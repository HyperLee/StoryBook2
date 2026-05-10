using Microsoft.AspNetCore.Mvc.RazorPages;
using StoryBook.Models;
using StoryBook.Services;

namespace StoryBook.Pages.Aquarium;

public sealed class DetailsModel : PageModel
{
    private readonly AquariumCatalogService _catalogService;

    public DetailsModel(AquariumCatalogService catalogService)
    {
        _catalogService = catalogService;
    }

    public AquariumAnimalProfile? Profile { get; private set; }

    public AquariumHabitatCategory? HabitatCategory { get; private set; }

    public AquariumAnimalProfile? PreviousProfile { get; private set; }

    public AquariumAnimalProfile? NextProfile { get; private set; }

    public bool HasLoadFailure { get; private set; }

    public bool IsNotFound { get; private set; }

    public void OnGet(string slug)
    {
        ViewData["UseAquariumAssets"] = true;

        try
        {
            if (_catalogService.TryGetBySlug(slug, out AquariumAnimalProfile? profile) && profile is not null)
            {
                Profile = profile;
                PreviousProfile = _catalogService.GetPreviousProfile(slug);
                NextProfile = _catalogService.GetNextProfile(slug);
                HabitatCategory = _catalogService.GetHabitatCategories()
                    .FirstOrDefault(category => category.Code == profile.HabitatCategory);
                return;
            }

            IsNotFound = true;
            Response.StatusCode = StatusCodes.Status404NotFound;
        }
        catch (InvalidOperationException)
        {
            HasLoadFailure = true;
        }
    }
}
