using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using StoryBook.Models;

namespace StoryBook.Services;

/// <summary>
/// Loads, caches, filters, orders, and projects local quiz questions.
/// </summary>
public sealed class QuizCatalogService
{
    private readonly QuizCatalogOptions _options;
    private readonly IWebHostEnvironment _environment;
    private readonly QuizContentValidator _validator;
    private readonly DinosaurCatalogService _dinosaurCatalogService;
    private readonly AquariumCatalogService _aquariumCatalogService;
    private readonly ILogger<QuizCatalogService> _logger;
    private readonly Lazy<QuizCatalogSnapshot> _snapshot;

    /// <summary>
    /// Creates the quiz catalog service using options, environment paths, validation, and logging.
    /// </summary>
    public QuizCatalogService(
        IOptions<QuizCatalogOptions> options,
        IWebHostEnvironment environment,
        QuizContentValidator validator,
        DinosaurCatalogService dinosaurCatalogService,
        AquariumCatalogService aquariumCatalogService,
        ILogger<QuizCatalogService> logger)
    {
        _options = options.Value;
        _environment = environment;
        _validator = validator;
        _dinosaurCatalogService = dinosaurCatalogService;
        _aquariumCatalogService = aquariumCatalogService;
        _logger = logger;
        _snapshot = new Lazy<QuizCatalogSnapshot>(LoadSnapshot);
    }

    /// <summary>
    /// Gets the cached catalog snapshot.
    /// </summary>
    public QuizCatalogSnapshot GetSnapshot()
    {
        return _snapshot.Value;
    }

    /// <summary>
    /// Gets safe question projections in the selected scope and language.
    /// </summary>
    public IReadOnlyList<QuizQuestionView> GetQuestionViews(QuizScope scope, LanguageCode language)
    {
        return GetScopedQuestions(scope)
            .Select(question => CreateQuestionView(question, scope, language))
            .ToList();
    }

    /// <summary>
    /// Gets the requested question, or the first available question in scope when missing or unknown.
    /// </summary>
    public QuizQuestionView? GetQuestionView(QuizScope scope, string? questionId, LanguageCode language)
    {
        IReadOnlyList<QuizQuestionView> questions = GetQuestionViews(scope, language);

        if (questions.Count == 0)
        {
            return null;
        }

        string normalizedQuestionId = NormalizeId(questionId);
        return questions.FirstOrDefault(question => question.Id == normalizedQuestionId) ?? questions[0];
    }

    /// <summary>
    /// Gets the next question id in the selected scope, cycling to the first question.
    /// </summary>
    public string? GetNextQuestionId(QuizScope scope, string? questionId)
    {
        IReadOnlyList<QuizQuestion> questions = GetScopedQuestions(scope);

        if (questions.Count == 0)
        {
            return null;
        }

        string normalizedQuestionId = NormalizeId(questionId);
        int index = -1;

        for (int currentIndex = 0; currentIndex < questions.Count; currentIndex++)
        {
            if (questions[currentIndex].Id == normalizedQuestionId)
            {
                index = currentIndex;
                break;
            }
        }

        int nextIndex = index < 0 || index >= questions.Count - 1 ? 0 : index + 1;
        return questions[nextIndex].Id;
    }

    /// <summary>
    /// Evaluates one submitted answer without persisting answer state.
    /// </summary>
    public QuizAnswerResult EvaluateAnswer(
        QuizScope scope,
        string? questionId,
        string? selectedOptionId,
        LanguageCode language)
    {
        IReadOnlyList<QuizQuestion> questions = GetScopedQuestions(scope);
        QuizQuestion? question = questions.FirstOrDefault(item => item.Id == NormalizeId(questionId))
            ?? questions.FirstOrDefault();

        if (question is null)
        {
            return CreateNeedsSelectionResult(
                NormalizeId(questionId),
                selectedOptionId);
        }

        string? normalizedSelectedOptionId = string.IsNullOrWhiteSpace(selectedOptionId)
            ? null
            : NormalizeId(selectedOptionId);
        QuizAnswerOption? selectedOption = normalizedSelectedOptionId is null
            ? null
            : question.Options.FirstOrDefault(option => option.Id == normalizedSelectedOptionId);

        if (selectedOption is null)
        {
            return CreateNeedsSelectionResult(question.Id, normalizedSelectedOptionId);
        }

        bool isCorrect = string.Equals(question.CorrectOptionId, selectedOption.Id, StringComparison.Ordinal);
        QuizText feedback = isCorrect ? question.CorrectFeedback : question.IncorrectFeedback;

        return new QuizAnswerResult
        {
            QuestionId = question.Id,
            SelectedOptionId = selectedOption.Id,
            IsAnswered = true,
            IsCorrect = isCorrect,
            FeedbackZhTW = feedback.Get(LanguageCode.ZhTW),
            FeedbackEn = feedback.Get(LanguageCode.En),
            ExplanationZhTW = question.Explanation.Get(LanguageCode.ZhTW),
            ExplanationEn = question.Explanation.Get(LanguageCode.En)
        };
    }

    /// <summary>
    /// Builds a canonical same-page question href.
    /// </summary>
    public static string BuildQuestionHref(QuizScope scope, string questionId)
    {
        return $"/quiz?scope={scope.ToCode()}&questionId={Uri.EscapeDataString(questionId)}";
    }

    private QuizCatalogSnapshot LoadSnapshot()
    {
        try
        {
            string json = File.ReadAllText(ResolveContentPath());
            QuizCatalog? catalog = JsonSerializer.Deserialize<QuizCatalog>(json, new JsonSerializerOptions(JsonSerializerDefaults.Web));
            QuizContentValidationResult result = _validator.ValidateCatalog(catalog);

            if (!result.IsCatalogUsable)
            {
                _logger.LogWarning("Quiz catalog root validation failed with {DiagnosticCount} diagnostics.", result.Diagnostics.Count);
                return CreateUnavailableSnapshot("catalog-invalid");
            }

            foreach (QuizContentDiagnostic diagnostic in result.Diagnostics)
            {
                _logger.LogWarning(
                    "Quiz content validation diagnostic {ReasonCode} for question {QuestionId} and source {SourceCode}.",
                    diagnostic.ReasonCode,
                    diagnostic.QuestionId ?? "catalog",
                    diagnostic.SourceCode ?? "all");
            }

            IReadOnlyList<QuizQuestion> candidateQuestions = SortQuestions(result.ValidQuestions)
                .Where(question => TryParseSource(question.Source, out _))
                .Where(question => QuizDifficultyParser.TryParse(question.Difficulty, out _))
                .ToList();
            List<QuizQuestion> questions = [];
            int invalidReferenceCount = 0;

            foreach (QuizQuestion question in candidateQuestions)
            {
                if (ResolveRelatedStories(question).Count == 0)
                {
                    invalidReferenceCount++;
                    _logger.LogWarning(
                        "Quiz question {QuestionId} rejected. Reason: {ReasonCode}",
                        question.Id,
                        "related-story-invalid");
                    continue;
                }

                questions.Add(question);
            }

            IReadOnlyList<QuizSourceStatus> statuses = CreateSourceStatuses(questions);
            return new QuizCatalogSnapshot(
                questions,
                statuses,
                result.InvalidQuestionCount + invalidReferenceCount);
        }
        catch (Exception exception) when (exception is IOException or JsonException or UnauthorizedAccessException)
        {
            _logger.LogWarning(exception, "Quiz catalog could not be loaded. Reason: {ReasonCode}", "catalog-load-failed");
            return CreateUnavailableSnapshot("catalog-load-failed");
        }
    }

    private IReadOnlyList<QuizQuestion> GetScopedQuestions(QuizScope scope)
    {
        return GetSnapshot()
            .Questions
            .Where(question => TryParseSource(question.Source, out ExplorationSourceType source) && scope.Includes(source))
            .ToList();
    }

    private QuizQuestionView CreateQuestionView(QuizQuestion question, QuizScope scope, LanguageCode language)
    {
        TryParseSource(question.Source, out ExplorationSourceType source);
        QuizDifficultyParser.TryParse(question.Difficulty, out QuizDifficulty difficulty);
        string nextQuestionId = GetNextQuestionId(scope, question.Id) ?? question.Id;

        return new QuizQuestionView
        {
            Id = question.Id,
            Scope = scope,
            Source = source,
            SourceLabelZhTW = source.GetLabel(LanguageCode.ZhTW),
            SourceLabelEn = source.GetLabel(LanguageCode.En),
            Difficulty = difficulty,
            PromptZhTW = question.Prompt.Get(LanguageCode.ZhTW),
            PromptEn = question.Prompt.Get(LanguageCode.En),
            Options = question.Options
                .OrderBy(option => option.SortOrder)
                .ThenBy(option => option.Id, StringComparer.Ordinal)
                .Select(option => new QuizAnswerOptionView
                {
                    Id = option.Id,
                    SortOrder = option.SortOrder,
                    TextZhTW = option.Text.Get(LanguageCode.ZhTW),
                    TextEn = option.Text.Get(LanguageCode.En)
                })
                .ToList(),
            RelatedStories = ResolveRelatedStories(question),
            NextQuestionHref = BuildQuestionHref(scope, nextQuestionId)
        };
    }

    private static IReadOnlyList<QuizQuestion> SortQuestions(IEnumerable<QuizQuestion> questions)
    {
        return questions
            .Where(question => !string.IsNullOrWhiteSpace(question.Id))
            .OrderBy(question => TryParseSource(question.Source, out ExplorationSourceType source)
                ? source.GetSortOrder()
                : int.MaxValue)
            .ThenBy(question => question.SortOrder)
            .ThenBy(question => question.Id, StringComparer.Ordinal)
            .ToList();
    }

    private IReadOnlyList<QuizRelatedStoryView> ResolveRelatedStories(QuizQuestion question)
    {
        if (question.RelatedStories.Count == 0)
        {
            return [];
        }

        HashSet<string> seenReferences = new(StringComparer.Ordinal);
        List<QuizRelatedStoryView> relatedStories = [];

        foreach (QuizStoryReference reference in question.RelatedStories
            .OrderBy(reference => reference.SortOrder)
            .ThenBy(reference => reference.Source, StringComparer.Ordinal)
            .ThenBy(reference => reference.Slug, StringComparer.Ordinal))
        {
            if (!TryParseSource(reference.Source, out ExplorationSourceType source))
            {
                return [];
            }

            string slug = NormalizeId(reference.Slug);
            string key = $"{source.ToCode()}:{slug}";

            if (string.IsNullOrWhiteSpace(slug) || !seenReferences.Add(key))
            {
                return [];
            }

            if (!TryGetStoryDetails(source, slug, out string nameZhTW, out string nameEn))
            {
                return [];
            }

            relatedStories.Add(new QuizRelatedStoryView
            {
                Source = source,
                Slug = slug,
                Href = $"/{source.GetRoutePrefix()}/{slug}",
                SortOrder = reference.SortOrder,
                LabelZhTW = $"去讀{nameZhTW}故事",
                LabelEn = $"Read the {nameEn} story",
                SourceLabelZhTW = source.GetLabel(LanguageCode.ZhTW),
                SourceLabelEn = source.GetLabel(LanguageCode.En)
            });
        }

        return relatedStories;
    }

    private bool TryGetStoryDetails(
        ExplorationSourceType source,
        string slug,
        out string nameZhTW,
        out string nameEn)
    {
        nameZhTW = string.Empty;
        nameEn = string.Empty;

        try
        {
            if (source == ExplorationSourceType.Aquarium)
            {
                if (!_aquariumCatalogService.TryGetBySlug(slug, out AquariumAnimalProfile? animal) || animal is null)
                {
                    return false;
                }

                nameZhTW = animal.Names.Get(LanguageCode.ZhTW);
                nameEn = animal.Names.Get(LanguageCode.En);
                return true;
            }

            if (!_dinosaurCatalogService.TryGetBySlug(slug, out DinosaurProfile? dinosaur) || dinosaur is null)
            {
                return false;
            }

            nameZhTW = dinosaur.Names.Get(LanguageCode.ZhTW);
            nameEn = dinosaur.Names.Get(LanguageCode.En);
            return true;
        }
        catch (Exception exception)
        {
            _logger.LogWarning(
                exception,
                "Quiz related story source {Source} unavailable. Reason: {ReasonCode}",
                source.ToCode(),
                "related-source-unavailable");
            return false;
        }
    }

    private IReadOnlyList<QuizSourceStatus> CreateSourceStatuses(IReadOnlyList<QuizQuestion> questions)
    {
        return
        [
            CreateSourceStatus(ExplorationSourceType.Dinosaurs, questions),
            CreateSourceStatus(ExplorationSourceType.Aquarium, questions)
        ];
    }

    private QuizSourceStatus CreateSourceStatus(ExplorationSourceType source, IReadOnlyList<QuizQuestion> questions)
    {
        if (!TryGetStoryCount(source, out int storyCount))
        {
            return CreateUnavailableStatus(source, "source-unavailable");
        }

        int questionCount = questions.Count(question => TryParseSource(question.Source, out ExplorationSourceType parsed) && parsed == source);

        return new QuizSourceStatus
        {
            Source = source,
            IsAvailable = true,
            StoryCount = storyCount,
            QuestionCount = questionCount,
            FriendlyMessageZhTW = questionCount == 0 ? $"{source.GetLabel(LanguageCode.ZhTW)}題目正在準備中。" : null,
            FriendlyMessageEn = questionCount == 0 ? $"{source.GetLabel(LanguageCode.En)} questions are being prepared." : null,
            ReasonCode = questionCount == 0 ? "source-empty" : null
        };
    }

    private bool TryGetStoryCount(ExplorationSourceType source, out int storyCount)
    {
        storyCount = 0;

        try
        {
            storyCount = source == ExplorationSourceType.Aquarium
                ? _aquariumCatalogService.GetProfiles().Count
                : _dinosaurCatalogService.GetProfiles().Count;
            return true;
        }
        catch (Exception exception)
        {
            _logger.LogWarning(
                exception,
                "Quiz source {Source} could not be counted. Reason: {ReasonCode}",
                source.ToCode(),
                "source-unavailable");
            return false;
        }
    }

    private static QuizCatalogSnapshot CreateUnavailableSnapshot(string reasonCode)
    {
        return new QuizCatalogSnapshot(
            [],
            [
                CreateUnavailableStatus(ExplorationSourceType.Dinosaurs, reasonCode),
                CreateUnavailableStatus(ExplorationSourceType.Aquarium, reasonCode)
            ],
            invalidQuestionCount: 0);
    }

    private static QuizSourceStatus CreateUnavailableStatus(ExplorationSourceType source, string reasonCode)
    {
        return new QuizSourceStatus
        {
            Source = source,
            IsAvailable = false,
            QuestionCount = 0,
            FriendlyMessageZhTW = $"{source.GetLabel(LanguageCode.ZhTW)}故事題目暫時躲起來了。",
            FriendlyMessageEn = $"{source.GetLabel(LanguageCode.En)} quiz questions are hiding for now.",
            ReasonCode = reasonCode
        };
    }

    private static bool TryParseSource(string? value, out ExplorationSourceType source)
    {
        string normalized = value?.Trim().ToLowerInvariant() ?? string.Empty;

        source = normalized switch
        {
            "aquarium" => ExplorationSourceType.Aquarium,
            "dinosaurs" => ExplorationSourceType.Dinosaurs,
            _ => default
        };

        return normalized is "aquarium" or "dinosaurs";
    }

    private string ResolveContentPath()
    {
        if (Path.IsPathRooted(_options.ContentPath))
        {
            return _options.ContentPath;
        }

        return Path.Combine(_environment.ContentRootPath, _options.ContentPath);
    }

    private static string NormalizeId(string? value)
    {
        return value?.Trim().ToLowerInvariant() ?? string.Empty;
    }

    private static QuizAnswerResult CreateNeedsSelectionResult(
        string questionId,
        string? selectedOptionId)
    {
        const string feedbackZhTW = "請先選一個答案，再送出挑戰。";
        const string feedbackEn = "Please choose one answer before submitting.";

        return new QuizAnswerResult
        {
            QuestionId = questionId,
            SelectedOptionId = selectedOptionId,
            IsAnswered = false,
            IsCorrect = null,
            FeedbackZhTW = feedbackZhTW,
            FeedbackEn = feedbackEn
        };
    }
}
