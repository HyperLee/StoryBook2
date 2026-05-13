using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StoryBook.Services;

namespace StoryBook.Pages;

public class IndexModel : PageModel
{
    private readonly ThemePreferenceService _themePreferenceService;

    public IndexModel(ThemePreferenceService themePreferenceService)
    {
        _themePreferenceService = themePreferenceService;
    }

    public string ThemeStorageKey => _themePreferenceService.StorageKey;

    public string DefaultThemeMode => _themePreferenceService.DefaultMode;

    public IReadOnlyList<ThemeModeMetadata> ThemeModes => _themePreferenceService.Modes;

    public void OnGet()
    {

    }
}
