namespace StoryBook.Models;

/// <summary>
/// Root object for the learning journey catalog JSON file.
/// </summary>
public sealed class JourneyCatalog
{
    /// <summary>
    /// Curated journey definitions.
    /// </summary>
    public List<LearningJourney> Journeys { get; init; } = [];
}
