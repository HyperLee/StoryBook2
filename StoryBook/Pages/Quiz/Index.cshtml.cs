using Microsoft.AspNetCore.Mvc.RazorPages;
using StoryBook.Models;
using StoryBook.Services;

namespace StoryBook.Pages.Quiz;

public sealed class IndexModel : PageModel
{
    private readonly QuizCatalogService _quizCatalogService;
    private readonly LanguagePreferenceService _languagePreferenceService;

    public IndexModel(
        QuizCatalogService quizCatalogService,
        LanguagePreferenceService languagePreferenceService)
    {
        _quizCatalogService = quizCatalogService;
        _languagePreferenceService = languagePreferenceService;
    }

    public QuizScope Scope { get; private set; } = QuizScope.All;

    public string ScopeCode => Scope.ToCode();

    public QuizQuestionView? CurrentQuestion { get; private set; }

    public IReadOnlyList<QuizQuestionView> QuestionsInScope { get; private set; } = [];

    public IReadOnlyList<QuizSourceStatus> SourceStatuses { get; private set; } = [];

    public bool HasAnyQuestion { get; private set; }

    public bool HasScopeFallback { get; private set; }

    public bool HasQuestionFallback { get; private set; }

    public string LanguageStorageKey => _languagePreferenceService.StorageKey;

    public void OnGet(string? scope, string? questionId)
    {
        ViewData["UseQuizAssets"] = true;

        Scope = QuizScopeParser.ParseOrDefault(scope);
        HasScopeFallback = IsInvalidExplicitScope(scope);

        QuizCatalogSnapshot snapshot = _quizCatalogService.GetSnapshot();
        SourceStatuses = snapshot.SourceStatuses;
        HasAnyQuestion = snapshot.HasAnyQuestion;
        QuestionsInScope = _quizCatalogService.GetQuestionViews(Scope, LanguageCode.ZhTW);
        CurrentQuestion = _quizCatalogService.GetQuestionView(Scope, questionId, LanguageCode.ZhTW);
        HasQuestionFallback = !string.IsNullOrWhiteSpace(questionId)
            && CurrentQuestion is not null
            && !string.Equals(CurrentQuestion.Id, questionId.Trim(), StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsInvalidExplicitScope(string? scope)
    {
        if (string.IsNullOrWhiteSpace(scope))
        {
            return false;
        }

        string normalized = scope.Trim().ToLowerInvariant();
        return normalized is not ("all" or "dinosaurs" or "aquarium");
    }
}
