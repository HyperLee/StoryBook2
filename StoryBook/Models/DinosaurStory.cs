namespace StoryBook.Models;

/// <summary>
/// A warm short story associated with one dinosaur profile.
/// </summary>
public sealed class DinosaurStory
{
    /// <summary>
    /// Bilingual story title.
    /// </summary>
    public DinosaurText Title { get; init; } = new();

    /// <summary>
    /// Bilingual story body.
    /// </summary>
    public DinosaurText Body { get; init; } = new();

    /// <summary>
    /// Story illustration and accessible text.
    /// </summary>
    public DinosaurIllustration Illustration { get; init; } = new();
}
