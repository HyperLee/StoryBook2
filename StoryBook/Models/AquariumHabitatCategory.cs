namespace StoryBook.Models;

/// <summary>
/// Represents an aquarium habitat category such as freshwater, coral reef, or polar waters.
/// </summary>
public sealed class AquariumHabitatCategory
{
    /// <summary>
    /// Unique lowercase kebab-case category code.
    /// </summary>
    public string Code { get; init; } = string.Empty;

    /// <summary>
    /// Display order for category lists.
    /// </summary>
    public int SortOrder { get; init; }

    /// <summary>
    /// Bilingual display name.
    /// </summary>
    public AquariumText DisplayName { get; init; } = new();

    /// <summary>
    /// Optional bilingual category description.
    /// </summary>
    public AquariumText? Description { get; init; }
}
