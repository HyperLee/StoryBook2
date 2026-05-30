namespace StoryBook.Models;

/// <summary>
/// User-selectable quiz question scope.
/// </summary>
public enum QuizScope
{
    /// <summary>
    /// All valid dinosaur and aquarium questions.
    /// </summary>
    All,

    /// <summary>
    /// Dinosaur questions only.
    /// </summary>
    Dinosaurs,

    /// <summary>
    /// Aquarium questions only.
    /// </summary>
    Aquarium
}

/// <summary>
/// Parses and formats quiz scope values used by routes, forms, and DOM metadata.
/// </summary>
public static class QuizScopeParser
{
    /// <summary>
    /// Parses a scope value, falling back to all stories for missing or invalid values.
    /// </summary>
    public static QuizScope ParseOrDefault(string? value)
    {
        string normalized = value?.Trim().ToLowerInvariant() ?? string.Empty;

        return normalized switch
        {
            "dinosaurs" => QuizScope.Dinosaurs,
            "aquarium" => QuizScope.Aquarium,
            "all" => QuizScope.All,
            _ => QuizScope.All
        };
    }

    /// <summary>
    /// Gets the stable lowercase scope code.
    /// </summary>
    public static string ToCode(this QuizScope scope)
    {
        return scope switch
        {
            QuizScope.Dinosaurs => "dinosaurs",
            QuizScope.Aquarium => "aquarium",
            _ => "all"
        };
    }

    /// <summary>
    /// Gets whether a story source is included in the selected scope.
    /// </summary>
    public static bool Includes(this QuizScope scope, ExplorationSourceType source)
    {
        return scope == QuizScope.All
            || (scope == QuizScope.Dinosaurs && source == ExplorationSourceType.Dinosaurs)
            || (scope == QuizScope.Aquarium && source == ExplorationSourceType.Aquarium);
    }
}
