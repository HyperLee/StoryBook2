namespace StoryBook.Models;

/// <summary>
/// A warm short story associated with one aquarium profile.
/// </summary>
public sealed class AquariumStory
{
    /// <summary>
    /// Bilingual story title.
    /// </summary>
    public AquariumText Title { get; init; } = new();

    /// <summary>
    /// Bilingual story body.
    /// </summary>
    public AquariumText Body { get; init; } = new();

    /// <summary>
    /// Story illustration and accessible text.
    /// </summary>
    public AquariumImage Illustration { get; init; } = new();
}
