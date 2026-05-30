using Microsoft.AspNetCore.Mvc.RazorPages;

namespace StoryBook.Pages.Quiz;

public sealed class IndexModel : PageModel
{
    public void OnGet()
    {
        ViewData["UseQuizAssets"] = true;
    }
}
