namespace StoryBook.Services;

/// <summary>
/// Result of validating the learning journey content catalog.
/// </summary>
public sealed class LearningJourneyContentValidationResult
{
    /// <summary>
    /// Creates a validation result from the supplied errors.
    /// </summary>
    public LearningJourneyContentValidationResult(IReadOnlyList<string> errors)
    {
        Errors = errors;
    }

    /// <summary>
    /// Validation errors safe for tests and developer diagnostics.
    /// </summary>
    public IReadOnlyList<string> Errors { get; }

    /// <summary>
    /// Whether validation completed without errors.
    /// </summary>
    public bool IsValid => Errors.Count == 0;
}
