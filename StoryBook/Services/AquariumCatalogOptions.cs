namespace StoryBook.Services;

/// <summary>
/// Options controlling where the aquarium catalog JSON file is loaded from.
/// </summary>
public sealed class AquariumCatalogOptions
{
    /// <summary>
    /// Configuration section name.
    /// </summary>
    public const string SectionName = "AquariumCatalog";

    /// <summary>
    /// Path to the catalog JSON file. Relative paths are resolved from the web content root.
    /// </summary>
    public string ContentPath { get; init; } = "Data/aquarium.json";
}
