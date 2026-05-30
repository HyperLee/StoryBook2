using System.Text.RegularExpressions;
using StoryBook.Models;

namespace StoryBook.Services;

/// <summary>
/// Validates the local quiz catalog before questions are projected to the UI.
/// </summary>
public sealed class QuizContentValidator
{
    private static readonly Regex KebabCasePattern = new("^[a-z0-9]+(?:-[a-z0-9]+)*$", RegexOptions.Compiled);

    /// <summary>
    /// Performs root-level quiz catalog validation.
    /// </summary>
    public QuizContentValidationResult ValidateCatalog(QuizCatalog? catalog)
    {
        if (catalog is null)
        {
            return QuizContentValidationResult.InvalidCatalog("catalog-null");
        }

        if (catalog.Version != 1)
        {
            return QuizContentValidationResult.InvalidCatalog("catalog-version");
        }

        if (catalog.Questions is null)
        {
            return QuizContentValidationResult.InvalidCatalog("catalog-questions-null");
        }

        List<QuizQuestion> validQuestions = [];
        List<QuizContentDiagnostic> diagnostics = [];
        HashSet<string> seenQuestionIds = new(StringComparer.Ordinal);

        foreach (QuizQuestion question in catalog.Questions)
        {
            int beforeCount = diagnostics.Count;
            ValidateQuestion(question, seenQuestionIds, diagnostics);

            if (diagnostics.Count == beforeCount)
            {
                validQuestions.Add(question);
            }
        }

        AddMinimumCountDiagnostics(validQuestions, diagnostics);

        return new QuizContentValidationResult(validQuestions, diagnostics, isCatalogUsable: true);
    }

    private static void ValidateQuestion(
        QuizQuestion question,
        HashSet<string> seenQuestionIds,
        List<QuizContentDiagnostic> diagnostics)
    {
        string questionId = question.Id?.Trim() ?? string.Empty;
        string? sourceCode = question.Source?.Trim().ToLowerInvariant();

        if (!IsKebabCase(questionId))
        {
            diagnostics.Add(new QuizContentDiagnostic("question-id-format", questionId, sourceCode));
        }
        else if (!seenQuestionIds.Add(questionId))
        {
            diagnostics.Add(new QuizContentDiagnostic("question-id-duplicate", questionId, sourceCode));
        }

        if (!IsValidSource(sourceCode))
        {
            diagnostics.Add(new QuizContentDiagnostic("question-source", questionId, sourceCode));
        }

        if (!QuizDifficultyParser.TryParse(question.Difficulty, out _))
        {
            diagnostics.Add(new QuizContentDiagnostic("question-difficulty", questionId, sourceCode));
        }

        if (question.SortOrder <= 0)
        {
            diagnostics.Add(new QuizContentDiagnostic("question-sort-order", questionId, sourceCode));
        }

        if (!IsCompleteText(question.Prompt))
        {
            diagnostics.Add(new QuizContentDiagnostic("question-prompt", questionId, sourceCode));
        }

        if (!IsCompleteText(question.CorrectFeedback))
        {
            diagnostics.Add(new QuizContentDiagnostic("question-correct-feedback", questionId, sourceCode));
        }

        if (!IsCompleteText(question.IncorrectFeedback))
        {
            diagnostics.Add(new QuizContentDiagnostic("question-incorrect-feedback", questionId, sourceCode));
        }

        if (!IsCompleteText(question.Explanation))
        {
            diagnostics.Add(new QuizContentDiagnostic("question-explanation", questionId, sourceCode));
        }

        ValidateOptions(question, questionId, sourceCode, diagnostics);
        ValidateRelatedStories(question, questionId, sourceCode, diagnostics);
    }

    private static void ValidateOptions(
        QuizQuestion question,
        string questionId,
        string? sourceCode,
        List<QuizContentDiagnostic> diagnostics)
    {
        if (question.Options is null || question.Options.Count is < 2 or > 4)
        {
            diagnostics.Add(new QuizContentDiagnostic("question-option-count", questionId, sourceCode));
            return;
        }

        HashSet<string> optionIds = new(StringComparer.Ordinal);

        foreach (QuizAnswerOption option in question.Options)
        {
            string optionId = option.Id?.Trim() ?? string.Empty;

            if (!IsKebabCase(optionId))
            {
                diagnostics.Add(new QuizContentDiagnostic("option-id-format", questionId, sourceCode));
            }
            else if (!optionIds.Add(optionId))
            {
                diagnostics.Add(new QuizContentDiagnostic("option-id-duplicate", questionId, sourceCode));
            }

            if (option.SortOrder <= 0)
            {
                diagnostics.Add(new QuizContentDiagnostic("option-sort-order", questionId, sourceCode));
            }

            if (!IsCompleteText(option.Text))
            {
                diagnostics.Add(new QuizContentDiagnostic("option-text", questionId, sourceCode));
            }
        }

        int correctOptionMatches = question.Options.Count(option =>
            string.Equals(option.Id, question.CorrectOptionId, StringComparison.Ordinal));

        if (!IsKebabCase(question.CorrectOptionId) || correctOptionMatches != 1)
        {
            diagnostics.Add(new QuizContentDiagnostic("question-correct-option", questionId, sourceCode));
        }
    }

    private static void ValidateRelatedStories(
        QuizQuestion question,
        string questionId,
        string? sourceCode,
        List<QuizContentDiagnostic> diagnostics)
    {
        if (question.RelatedStories is null || question.RelatedStories.Count == 0)
        {
            diagnostics.Add(new QuizContentDiagnostic("related-story-required", questionId, sourceCode));
            return;
        }

        HashSet<string> references = new(StringComparer.Ordinal);

        foreach (QuizStoryReference reference in question.RelatedStories)
        {
            string referenceSource = reference.Source?.Trim().ToLowerInvariant() ?? string.Empty;
            string slug = reference.Slug?.Trim().ToLowerInvariant() ?? string.Empty;

            if (!IsValidSource(referenceSource))
            {
                diagnostics.Add(new QuizContentDiagnostic("related-story-source", questionId, sourceCode));
            }

            if (!IsKebabCase(slug))
            {
                diagnostics.Add(new QuizContentDiagnostic("related-story-slug", questionId, sourceCode));
            }

            if (reference.SortOrder <= 0)
            {
                diagnostics.Add(new QuizContentDiagnostic("related-story-sort-order", questionId, sourceCode));
            }

            if (!string.IsNullOrWhiteSpace(referenceSource)
                && !string.IsNullOrWhiteSpace(slug)
                && !references.Add($"{referenceSource}:{slug}"))
            {
                diagnostics.Add(new QuizContentDiagnostic("related-story-duplicate", questionId, sourceCode));
            }
        }
    }

    private static void AddMinimumCountDiagnostics(
        IReadOnlyList<QuizQuestion> validQuestions,
        List<QuizContentDiagnostic> diagnostics)
    {
        if (validQuestions.Count < 12)
        {
            diagnostics.Add(new QuizContentDiagnostic("minimum-total", null, null));
        }

        int dinosaurCount = validQuestions.Count(question =>
            string.Equals(question.Source, "dinosaurs", StringComparison.OrdinalIgnoreCase));
        int aquariumCount = validQuestions.Count(question =>
            string.Equals(question.Source, "aquarium", StringComparison.OrdinalIgnoreCase));

        if (dinosaurCount < 5)
        {
            diagnostics.Add(new QuizContentDiagnostic("minimum-source-dinosaurs", null, "dinosaurs"));
        }

        if (aquariumCount < 5)
        {
            diagnostics.Add(new QuizContentDiagnostic("minimum-source-aquarium", null, "aquarium"));
        }
    }

    private static bool IsCompleteText(QuizText? text)
    {
        return text is not null
            && !string.IsNullOrWhiteSpace(text.ZhTW)
            && !string.IsNullOrWhiteSpace(text.En);
    }

    private static bool IsValidSource(string? source)
    {
        return source is "dinosaurs" or "aquarium";
    }

    private static bool IsKebabCase(string? value)
    {
        return !string.IsNullOrWhiteSpace(value) && KebabCasePattern.IsMatch(value);
    }
}
