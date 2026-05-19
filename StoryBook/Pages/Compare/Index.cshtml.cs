using Microsoft.AspNetCore.Mvc.RazorPages;
using StoryBook.Models;
using StoryBook.Services;

namespace StoryBook.Pages.Compare;

public sealed class IndexModel : PageModel
{
    private readonly IComparisonCatalogService _catalogService;

    public IndexModel(IComparisonCatalogService catalogService)
    {
        _catalogService = catalogService;
    }

    public IReadOnlyList<ComparisonCandidate> Candidates { get; private set; } = [];

    public IReadOnlyList<ComparisonFieldDefinition> FieldDefinitions { get; private set; } = [];

    public IReadOnlyList<ComparisonSourceStatus> SourceStatuses { get; private set; } = [];

    public bool HasEnoughCandidates { get; private set; }

    public bool HasPartialFailure { get; private set; }

    public bool HasAllSourcesFailed { get; private set; }

    public void OnGet()
    {
        ViewData["UseCompareAssets"] = true;

        ComparisonCatalogSnapshot snapshot = _catalogService.GetSnapshot();
        Candidates = snapshot.Candidates;
        FieldDefinitions = snapshot.FieldDefinitions;
        SourceStatuses = snapshot.SourceStatuses;
        HasEnoughCandidates = snapshot.HasEnoughCandidates;
        HasPartialFailure = snapshot.HasPartialFailure;
        HasAllSourcesFailed = snapshot.HasAllSourcesFailed;
    }
}
