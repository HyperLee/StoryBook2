using StoryBook.Models;
using StoryBook.Services;

namespace StoryBook.Tests.Unit;

public sealed class QuizContentValidationTests
{
    [Fact]
    public void ValidateCatalog_rejects_null_bad_version_and_missing_questions_root_shapes()
    {
        QuizContentValidator validator = new();

        Assert.False(validator.ValidateCatalog(null).IsCatalogUsable);
        Assert.Contains(validator.ValidateCatalog(null).Diagnostics, diagnostic => diagnostic.ReasonCode == "catalog-null");
        Assert.False(validator.ValidateCatalog(new QuizCatalog { Version = 2, Questions = [] }).IsCatalogUsable);
        Assert.Contains(
            validator.ValidateCatalog(new QuizCatalog { Version = 1, Questions = null! }).Diagnostics,
            diagnostic => diagnostic.ReasonCode == "catalog-questions-null");
    }

    [Fact]
    public void ValidateCatalog_filters_invalid_question_shapes_and_reports_reason_codes()
    {
        QuizQuestion valid = CreateValidQuestion("valid-dino", "dinosaurs");
        QuizQuestion invalidId = CreateValidQuestion("Invalid Id", "dinosaurs");
        QuizQuestion duplicateId = CreateValidQuestion("valid-dino", "dinosaurs");
        QuizQuestion invalidSource = CreateValidQuestion("invalid-source", "space");
        QuizQuestion invalidDifficulty = CreateValidQuestion("invalid-difficulty", "dinosaurs", difficulty: "hard");
        QuizQuestion missingText = CreateValidQuestion("missing-text", "dinosaurs", prompt: new QuizText { ZhTW = "", En = "" });
        QuizQuestion tooFewOptions = CreateValidQuestion("few-options", "dinosaurs", options: [CreateOption("one", 1)]);
        QuizQuestion missingCorrect = CreateValidQuestion("missing-correct", "dinosaurs", correctOptionId: "missing");
        QuizQuestion badReference = CreateValidQuestion(
            "bad-reference",
            "dinosaurs",
            relatedStories: [new QuizStoryReference { Source = "dinosaurs", Slug = "Not A Slug", SortOrder = 1 }]);
        QuizContentValidator validator = new();

        QuizContentValidationResult result = validator.ValidateCatalog(new QuizCatalog
        {
            Version = 1,
            Questions =
            [
                valid,
                invalidId,
                duplicateId,
                invalidSource,
                invalidDifficulty,
                missingText,
                tooFewOptions,
                missingCorrect,
                badReference
            ]
        });

        QuizQuestion remaining = Assert.Single(result.ValidQuestions);
        Assert.Equal("valid-dino", remaining.Id);
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.ReasonCode == "question-id-format");
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.ReasonCode == "question-id-duplicate");
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.ReasonCode == "question-source");
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.ReasonCode == "question-difficulty");
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.ReasonCode == "question-prompt");
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.ReasonCode == "question-option-count");
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.ReasonCode == "question-correct-option");
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.ReasonCode == "related-story-slug");
    }

    [Fact]
    public void ValidateCatalog_reports_minimum_total_and_source_counts_without_discarding_valid_questions()
    {
        QuizContentValidator validator = new();
        QuizCatalog catalog = new()
        {
            Version = 1,
            Questions =
            [
                CreateValidQuestion("dino-one", "dinosaurs"),
                CreateValidQuestion("aquarium-one", "aquarium")
            ]
        };

        QuizContentValidationResult result = validator.ValidateCatalog(catalog);

        Assert.Equal(2, result.ValidQuestions.Count);
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.ReasonCode == "minimum-total");
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.ReasonCode == "minimum-source-dinosaurs");
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.ReasonCode == "minimum-source-aquarium");
    }

    private static QuizQuestion CreateValidQuestion(
        string id,
        string source,
        string difficulty = "easy",
        QuizText? prompt = null,
        IReadOnlyList<QuizAnswerOption>? options = null,
        string correctOptionId = "one",
        IReadOnlyList<QuizStoryReference>? relatedStories = null)
    {
        return new QuizQuestion
        {
            Id = id,
            Source = source,
            Difficulty = difficulty,
            SortOrder = 1,
            Prompt = prompt ?? new QuizText { ZhTW = "題目", En = "Question" },
            Options = options ??
                [
                    CreateOption("one", 1),
                    CreateOption("two", 2)
                ],
            CorrectOptionId = correctOptionId,
            CorrectFeedback = new QuizText { ZhTW = "答對了", En = "Correct" },
            IncorrectFeedback = new QuizText { ZhTW = "再想想", En = "Try again" },
            Explanation = new QuizText { ZhTW = "這是解釋。", En = "This is the explanation." },
            RelatedStories = relatedStories ??
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

    private static QuizAnswerOption CreateOption(string id, int sortOrder)
    {
        return new QuizAnswerOption
        {
            Id = id,
            SortOrder = sortOrder,
            Text = new QuizText { ZhTW = $"選項 {id}", En = $"Option {id}" }
        };
    }
}
