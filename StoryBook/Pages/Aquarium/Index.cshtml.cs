using Microsoft.AspNetCore.Mvc.RazorPages;

namespace StoryBook.Pages.Aquarium;

public sealed class IndexModel : PageModel
{
    public void OnGet()
    {
        ViewData["UseAquariumAssets"] = true;
    }
}
