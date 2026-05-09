namespace StoryBook.Services;

/// <summary>
/// Options controlling where the dinosaur catalog JSON file is loaded from.
/// </summary>
public sealed class DinosaurCatalogOptions
{
    /// <summary>
    /// Configuration section name.
    /// </summary>
    public const string SectionName = "DinosaurCatalog";

    /// <summary>
    /// Path to the catalog JSON file. Relative paths are resolved from the web content root.
    /// </summary>
    public string ContentPath { get; init; } = "Data/dinosaurs.json";
}
