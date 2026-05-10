using Microsoft.AspNetCore.Mvc.RazorPages;

namespace StoryBook.Pages.Aquarium;

public sealed class DetailsModel : PageModel
{
    public void OnGet(string slug)
    {
        ViewData["UseAquariumAssets"] = true;
    }
}
