namespace StoryBook.Services;

/// <summary>
/// Result of validating the dinosaur content catalog.
/// </summary>
public sealed class DinosaurContentValidationResult
{
    /// <summary>
    /// Creates a validation result from all discovered error messages.
    /// </summary>
    public DinosaurContentValidationResult(IReadOnlyList<string> errors)
    {
        Errors = errors;
    }

    /// <summary>
    /// Gets whether validation found no errors.
    /// </summary>
    public bool IsValid => Errors.Count == 0;

    /// <summary>
    /// Gets validation error messages.
    /// </summary>
    public IReadOnlyList<string> Errors { get; }
}
