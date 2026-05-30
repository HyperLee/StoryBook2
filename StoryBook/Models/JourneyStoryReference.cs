namespace StoryBook.Models;

/// <summary>
/// Identifies one existing story item referenced by a learning journey.
/// </summary>
public sealed class JourneyStoryReference
{
    /// <summary>
    /// Stable storybook source code, currently <c>dinosaurs</c> or <c>aquarium</c>.
    /// </summary>
    public string Source { get; init; } = string.Empty;

    /// <summary>
    /// Slug of the existing story item inside the source catalog.
    /// </summary>
    public string Slug { get; init; } = string.Empty;

    /// <summary>
    /// Reading order inside the journey.
    /// </summary>
    public int SortOrder { get; init; }
}
