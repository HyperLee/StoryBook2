namespace StoryBook.Models;

/// <summary>
/// Supported quiz question difficulty levels.
/// </summary>
public enum QuizDifficulty
{
    /// <summary>
    /// Low-friction recall question.
    /// </summary>
    Easy,

    /// <summary>
    /// Comparison or inference question.
    /// </summary>
    Medium
}

/// <summary>
/// Parses and formats quiz difficulty values used by the JSON catalog and UI metadata.
/// </summary>
public static class QuizDifficultyParser
{
    /// <summary>
    /// Attempts to parse a supported difficulty code.
    /// </summary>
    public static bool TryParse(string? value, out QuizDifficulty difficulty)
    {
        string normalized = value?.Trim().ToLowerInvariant() ?? string.Empty;

        difficulty = normalized switch
        {
            "easy" => QuizDifficulty.Easy,
            "medium" => QuizDifficulty.Medium,
            _ => default
        };

        return normalized is "easy" or "medium";
    }

    /// <summary>
    /// Gets the stable lowercase code for a difficulty value.
    /// </summary>
    public static string ToCode(this QuizDifficulty difficulty)
    {
        return difficulty == QuizDifficulty.Medium ? "medium" : "easy";
    }
}
