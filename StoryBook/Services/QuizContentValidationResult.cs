using StoryBook.Models;

namespace StoryBook.Services;

/// <summary>
/// Result of quiz catalog content validation.
/// </summary>
public sealed class QuizContentValidationResult
{
    /// <summary>
    /// Creates a validation result.
    /// </summary>
    public QuizContentValidationResult(
        IReadOnlyList<QuizQuestion> validQuestions,
        IReadOnlyList<QuizContentDiagnostic> diagnostics,
        bool isCatalogUsable)
    {
        ValidQuestions = validQuestions;
        Diagnostics = diagnostics;
        IsCatalogUsable = isCatalogUsable;
    }

    /// <summary>
    /// Questions that passed root and per-question validation.
    /// </summary>
    public IReadOnlyList<QuizQuestion> ValidQuestions { get; }

    /// <summary>
    /// Non-sensitive diagnostics for rejected questions or catalog source failures.
    /// </summary>
    public IReadOnlyList<QuizContentDiagnostic> Diagnostics { get; }

    /// <summary>
    /// Whether the root catalog shape and schema version were usable.
    /// </summary>
    public bool IsCatalogUsable { get; }

    /// <summary>
    /// Whether there are no validation diagnostics.
    /// </summary>
    public bool IsValid => IsCatalogUsable && Diagnostics.Count == 0;

    /// <summary>
    /// Number of rejected question diagnostics.
    /// </summary>
    public int InvalidQuestionCount => Diagnostics
        .Where(diagnostic => !string.IsNullOrWhiteSpace(diagnostic.QuestionId))
        .Select(diagnostic => diagnostic.QuestionId)
        .Distinct(StringComparer.Ordinal)
        .Count();

    /// <summary>
    /// Creates a root-level invalid result.
    /// </summary>
    public static QuizContentValidationResult InvalidCatalog(string reasonCode)
    {
        return new QuizContentValidationResult(
            [],
            [new QuizContentDiagnostic(reasonCode, null, null)],
            isCatalogUsable: false);
    }
}

/// <summary>
/// Non-sensitive validation diagnostic for quiz content.
/// </summary>
public sealed record QuizContentDiagnostic(string ReasonCode, string? QuestionId, string? SourceCode);
