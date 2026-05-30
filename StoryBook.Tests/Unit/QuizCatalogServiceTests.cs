using System.Text.Json;
using Microsoft.Extensions.Options;
using StoryBook.Models;
using StoryBook.Services;
using StoryBook.Tests.Support;

namespace StoryBook.Tests.Unit;

public sealed class QuizCatalogServiceTests
{
    [Fact]
    public void QuizScopeParser_supports_known_scopes_and_falls_back_to_all()
    {
        Assert.Equal(QuizScope.All, QuizScopeParser.ParseOrDefault("all"));
        Assert.Equal(QuizScope.Dinosaurs, QuizScopeParser.ParseOrDefault("dinosaurs"));
        Assert.Equal(QuizScope.Aquarium, QuizScopeParser.ParseOrDefault("aquarium"));
        Assert.Equal(QuizScope.All, QuizScopeParser.ParseOrDefault("space"));
        Assert.Equal(QuizScope.All, QuizScopeParser.ParseOrDefault(null));
    }

    [Fact]
    public void GetQuestionViews_uses_stable_source_sort_order_sort_order_and_id()
    {
        using QuizCatalogServiceTestContext context = CreateContext(
            CreateQuestion("aquarium-first", "aquarium", 1),
            CreateQuestion("dinosaurs-second-id", "dinosaurs", 2),
            CreateQuestion("dinosaurs-first-b", "dinosaurs", 1),
            CreateQuestion("dinosaurs-first-a", "dinosaurs", 1));

        IReadOnlyList<QuizQuestionView> questions = context.Service.GetQuestionViews(QuizScope.All, LanguageCode.ZhTW);

        Assert.Equal(
            ["dinosaurs-first-a", "dinosaurs-first-b", "dinosaurs-second-id", "aquarium-first"],
            questions.Select(question => question.Id));
    }

    [Fact]
    public void GetQuestionViews_filters_by_scope_and_invalid_scope_falls_back_to_all()
    {
        using QuizCatalogServiceTestContext context = CreateContext(
            CreateQuestion("aquarium-question", "aquarium", 1),
            CreateQuestion("dinosaurs-question", "dinosaurs", 1));

        IReadOnlyList<QuizQuestionView> dinosaurQuestions = context.Service.GetQuestionViews(QuizScope.Dinosaurs, LanguageCode.ZhTW);
        IReadOnlyList<QuizQuestionView> aquariumQuestions = context.Service.GetQuestionViews(QuizScope.Aquarium, LanguageCode.ZhTW);
        QuizScope fallbackScope = QuizScopeParser.ParseOrDefault("invalid");
        IReadOnlyList<QuizQuestionView> fallbackQuestions = context.Service.GetQuestionViews(fallbackScope, LanguageCode.ZhTW);

        Assert.All(dinosaurQuestions, question => Assert.Equal("dinosaurs", question.SourceCode));
        Assert.All(aquariumQuestions, question => Assert.Equal("aquarium", question.SourceCode));
        Assert.Equal(2, fallbackQuestions.Count);
    }

    [Fact]
    public void GetNextQuestionId_cycles_within_selected_scope_and_uses_first_for_unknown_question()
    {
        using QuizCatalogServiceTestContext context = CreateContext(
            CreateQuestion("a-dino", "dinosaurs", 1),
            CreateQuestion("b-dino", "dinosaurs", 2),
            CreateQuestion("a-aquarium", "aquarium", 1));

        Assert.Equal("b-dino", context.Service.GetNextQuestionId(QuizScope.Dinosaurs, "a-dino"));
        Assert.Equal("a-dino", context.Service.GetNextQuestionId(QuizScope.Dinosaurs, "b-dino"));
        Assert.Equal("a-dino", context.Service.GetNextQuestionId(QuizScope.Dinosaurs, "unknown"));
        Assert.Equal("a-aquarium", context.Service.GetNextQuestionId(QuizScope.Aquarium, "a-aquarium"));
    }

    [Fact]
    public void EvaluateAnswer_returns_correct_feedback_and_explanation_without_persisting_state()
    {
        using QuizCatalogServiceTestContext context = CreateContext(CreateQuestion("a-dino", "dinosaurs", 1));

        QuizAnswerResult result = context.Service.EvaluateAnswer(QuizScope.Dinosaurs, "a-dino", "one", LanguageCode.ZhTW);
        QuizQuestionView viewAfterAnswer = Assert.Single(context.Service.GetQuestionViews(QuizScope.Dinosaurs, LanguageCode.ZhTW));

        Assert.True(result.IsAnswered);
        Assert.True(result.IsCorrect);
        Assert.Equal("one", result.SelectedOptionId);
        Assert.Equal("答對了", result.GetFeedback(LanguageCode.ZhTW));
        Assert.Equal("因為故事這樣說。", result.GetExplanation(LanguageCode.ZhTW));
        Assert.DoesNotContain("CorrectOptionId", JsonSerializer.Serialize(viewAfterAnswer));
        Assert.DoesNotContain("one", viewAfterAnswer.NextQuestionHref, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void EvaluateAnswer_returns_kind_wrong_feedback_for_wrong_answer()
    {
        using QuizCatalogServiceTestContext context = CreateContext(CreateQuestion("a-dino", "dinosaurs", 1));

        QuizAnswerResult result = context.Service.EvaluateAnswer(QuizScope.Dinosaurs, "a-dino", "two", LanguageCode.En);

        Assert.True(result.IsAnswered);
        Assert.False(result.IsCorrect);
        Assert.Equal("two", result.SelectedOptionId);
        Assert.Equal("Try again", result.GetFeedback(LanguageCode.En));
        Assert.Equal("The story says so.", result.GetExplanation(LanguageCode.En));
    }

    [Fact]
    public void EvaluateAnswer_treats_unknown_or_missing_option_as_not_answered()
    {
        using QuizCatalogServiceTestContext context = CreateContext(CreateQuestion("a-dino", "dinosaurs", 1));

        QuizAnswerResult missing = context.Service.EvaluateAnswer(QuizScope.Dinosaurs, "a-dino", null, LanguageCode.ZhTW);
        QuizAnswerResult unknown = context.Service.EvaluateAnswer(QuizScope.Dinosaurs, "a-dino", "missing-option", LanguageCode.En);

        Assert.False(missing.IsAnswered);
        Assert.Null(missing.IsCorrect);
        Assert.Null(missing.SelectedOptionId);
        Assert.Contains("請先選一個答案", missing.GetFeedback(LanguageCode.ZhTW));
        Assert.Null(missing.GetExplanation(LanguageCode.ZhTW));
        Assert.False(unknown.IsAnswered);
        Assert.Null(unknown.IsCorrect);
        Assert.Equal("missing-option", unknown.SelectedOptionId);
        Assert.Contains("choose one answer", unknown.GetFeedback(LanguageCode.En), StringComparison.OrdinalIgnoreCase);
    }

    private static QuizCatalogServiceTestContext CreateContext(params QuizQuestion[] questions)
    {
        string path = Path.Combine(Path.GetTempPath(), $"storybook-quiz-{Guid.NewGuid():N}.json");
        QuizCatalog catalog = new()
        {
            Version = 1,
            Questions = questions
        };
        string json = JsonSerializer.Serialize(catalog, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        File.WriteAllText(path, json);

        QuizCatalogService service = new(
            Options.Create(new QuizCatalogOptions { ContentPath = path }),
            new FakeWebHostEnvironment(TestPaths.StoryBookRoot),
            new QuizContentValidator(),
            new RecordingLogger<QuizCatalogService>());

        return new QuizCatalogServiceTestContext(path, service);
    }

    private static QuizQuestion CreateQuestion(string id, string source, int sortOrder)
    {
        return new QuizQuestion
        {
            Id = id,
            Source = source,
            Difficulty = "easy",
            SortOrder = sortOrder,
            Prompt = new QuizText { ZhTW = $"題目 {id}", En = $"Question {id}" },
            Options =
            [
                new QuizAnswerOption
                {
                    Id = "one",
                    SortOrder = 1,
                    Text = new QuizText { ZhTW = "第一個答案", En = "First answer" }
                },
                new QuizAnswerOption
                {
                    Id = "two",
                    SortOrder = 2,
                    Text = new QuizText { ZhTW = "第二個答案", En = "Second answer" }
                }
            ],
            CorrectOptionId = "one",
            CorrectFeedback = new QuizText { ZhTW = "答對了", En = "Correct" },
            IncorrectFeedback = new QuizText { ZhTW = "再想想", En = "Try again" },
            Explanation = new QuizText { ZhTW = "因為故事這樣說。", En = "The story says so." },
            RelatedStories =
            [
                new QuizStoryReference
                {
                    Source = source,
                    Slug = source == "aquarium" ? "clownfish" : "triceratops",
                    SortOrder = 1
                }
            ]
        };
    }

    private sealed class QuizCatalogServiceTestContext : IDisposable
    {
        private readonly string _contentPath;

        public QuizCatalogServiceTestContext(string contentPath, QuizCatalogService service)
        {
            _contentPath = contentPath;
            Service = service;
        }

        public QuizCatalogService Service { get; }

        public void Dispose()
        {
            if (File.Exists(_contentPath))
            {
                File.Delete(_contentPath);
            }
        }
    }
}
