namespace StoryBook.Services;

/// <summary>
/// Options controlling where the learning journey catalog JSON file is loaded from.
/// </summary>
public sealed class LearningJourneyCatalogOptions
{
    /// <summary>
    /// Configuration section name.
    /// </summary>
    public const string SectionName = "LearningJourneyCatalog";

    /// <summary>
    /// Path to the catalog JSON file. Relative paths are resolved from the web content root.
    /// </summary>
    public string ContentPath { get; init; } = "Data/journeys.json";
}
