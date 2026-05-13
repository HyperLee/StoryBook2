using Microsoft.AspNetCore.Mvc.RazorPages;

namespace StoryBook.Pages.Explore;

public sealed class IndexModel : PageModel
{
    public void OnGet()
    {
        ViewData["UseExploreAssets"] = true;
    }
}
