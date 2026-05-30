namespace StoryBook.Models;

/// <summary>
/// Represents one curated learning journey built from existing storybook items.
/// </summary>
public sealed class LearningJourney
{
    /// <summary>
    /// Unique lowercase kebab-case slug used by <c>/journeys/{slug}</c>.
    /// </summary>
    public string Slug { get; init; } = string.Empty;

    /// <summary>
    /// Display and list order.
    /// </summary>
    public int SortOrder { get; init; }

    /// <summary>
    /// Bilingual journey title.
    /// </summary>
    public JourneyText Title { get; init; } = new();

    /// <summary>
    /// Bilingual child-friendly journey summary.
    /// </summary>
    public JourneyText Summary { get; init; } = new();

    /// <summary>
    /// Bilingual learning goals shown on list and detail pages.
    /// </summary>
    public List<JourneyText> LearningGoals { get; init; } = [];

    /// <summary>
    /// Suggested reading time in minutes.
    /// </summary>
    public int SuggestedReadingMinutes { get; init; }

    /// <summary>
    /// Bilingual age guidance text.
    /// </summary>
    public JourneyText AgeGuidance { get; init; } = new();

    /// <summary>
    /// Existing story references that make up the journey.
    /// </summary>
    public List<JourneyStoryReference> StoryReferences { get; init; } = [];
}
