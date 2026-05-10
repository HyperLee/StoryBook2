namespace StoryBook.Models;

/// <summary>
/// Root object for the local aquarium content catalog.
/// </summary>
public sealed class AquariumCatalog
{
    /// <summary>
    /// Catalog version for content review and cache diagnostics.
    /// </summary>
    public string Version { get; init; } = string.Empty;

    /// <summary>
    /// Habitat categories used by aquarium profiles.
    /// </summary>
    public List<AquariumHabitatCategory> HabitatCategories { get; init; } = [];

    /// <summary>
    /// All aquarium animal profiles.
    /// </summary>
    public List<AquariumAnimalProfile> Animals { get; init; } = [];
}
