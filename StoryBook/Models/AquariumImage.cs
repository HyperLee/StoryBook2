namespace StoryBook.Models;

/// <summary>
/// Describes a child-friendly aquarium illustration and its bilingual accessibility text.
/// </summary>
public sealed class AquariumImage
{
    /// <summary>
    /// Site-relative image path under <c>/images/aquarium/</c>.
    /// </summary>
    public string Path { get; init; } = string.Empty;

    /// <summary>
    /// Bilingual alternative text for the image.
    /// </summary>
    public AquariumText AltText { get; init; } = new();

    /// <summary>
    /// Optional bilingual caption displayed near the image.
    /// </summary>
    public AquariumText? Caption { get; init; }

    /// <summary>
    /// Visual style marker used to verify a consistent storybook illustration style.
    /// </summary>
    public string StyleTag { get; init; } = string.Empty;
}
