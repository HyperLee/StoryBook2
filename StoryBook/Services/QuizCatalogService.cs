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
    private readonly ILogger<QuizCatalogService> _logger;
    private readonly Lazy<QuizCatalogSnapshot> _snapshot;

    /// <summary>
    /// Creates the quiz catalog service using options, environment paths, validation, and logging.
    /// </summary>
    public QuizCatalogService(
        IOptions<QuizCatalogOptions> options,
        IWebHostEnvironment environment,
        QuizContentValidator validator,
        ILogger<QuizCatalogService> logger)
    {
        _options = options.Value;
        _environment = environment;
        _validator = validator;
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

            IReadOnlyList<QuizQuestion> questions = SortQuestions(result.ValidQuestions)
                .Where(question => TryParseSource(question.Source, out _))
                .Where(question => QuizDifficultyParser.TryParse(question.Difficulty, out _))
                .ToList();

            IReadOnlyList<QuizSourceStatus> statuses = CreateAvailableStatuses(questions);
            return new QuizCatalogSnapshot(questions, statuses, result.InvalidQuestionCount);
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
            RelatedStories = [],
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

    private static IReadOnlyList<QuizSourceStatus> CreateAvailableStatuses(IReadOnlyList<QuizQuestion> questions)
    {
        return
        [
            CreateAvailableStatus(ExplorationSourceType.Dinosaurs, questions),
            CreateAvailableStatus(ExplorationSourceType.Aquarium, questions)
        ];
    }

    private static QuizSourceStatus CreateAvailableStatus(ExplorationSourceType source, IReadOnlyList<QuizQuestion> questions)
    {
        int questionCount = questions.Count(question => TryParseSource(question.Source, out ExplorationSourceType parsed) && parsed == source);

        return new QuizSourceStatus
        {
            Source = source,
            IsAvailable = true,
            QuestionCount = questionCount,
            FriendlyMessageZhTW = questionCount == 0 ? $"{source.GetLabel(LanguageCode.ZhTW)}題目正在準備中。" : null,
            FriendlyMessageEn = questionCount == 0 ? $"{source.GetLabel(LanguageCode.En)} questions are being prepared." : null,
            ReasonCode = questionCount == 0 ? "source-empty" : null
        };
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
}
