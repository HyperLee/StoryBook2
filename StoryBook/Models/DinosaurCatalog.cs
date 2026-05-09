namespace StoryBook.Models;

/// <summary>
/// Root object for the local dinosaur content catalog.
/// </summary>
public sealed class DinosaurCatalog
{
    /// <summary>
    /// Catalog version for content review.
    /// </summary>
    public string Version { get; init; } = string.Empty;

    /// <summary>
    /// All dinosaur and prehistoric animal profiles.
    /// </summary>
    public List<DinosaurProfile> Profiles { get; init; } = [];
}
