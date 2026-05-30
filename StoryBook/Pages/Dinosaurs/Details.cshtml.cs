using Microsoft.AspNetCore.Mvc.RazorPages;
using StoryBook.Models;
using StoryBook.Services;

namespace StoryBook.Pages.Dinosaurs;

public sealed class DetailsModel : PageModel
{
    private readonly DinosaurCatalogService _catalogService;

    public DetailsModel(DinosaurCatalogService catalogService)
    {
        _catalogService = catalogService;
    }

    public DinosaurProfile? Profile { get; private set; }

    public DinosaurProfile? PreviousProfile { get; private set; }

    public DinosaurProfile? NextProfile { get; private set; }

    public bool IsNotFound { get; private set; }

    public void OnGet(string slug)
    {
        ViewData["UseDinosaurAssets"] = true;

        if (_catalogService.TryGetBySlug(slug, out DinosaurProfile? profile))
        {
            Profile = profile;
            ViewData["UsePassportAssets"] = true;
            PreviousProfile = _catalogService.GetPreviousProfile(slug);
            NextProfile = _catalogService.GetNextProfile(slug);
            return;
        }

        IsNotFound = true;
        Response.StatusCode = StatusCodes.Status404NotFound;
    }
}
