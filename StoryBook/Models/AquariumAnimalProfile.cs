namespace StoryBook.Models;

/// <summary>
/// Represents one browsable aquarium animal profile.
/// </summary>
public sealed class AquariumAnimalProfile
{
    /// <summary>
    /// Unique lowercase kebab-case slug used by <c>/aquarium/{slug}</c>.
    /// </summary>
    public string Slug { get; init; } = string.Empty;

    /// <summary>
    /// Habitat category code matching <see cref="AquariumHabitatCategory.Code" />.
    /// </summary>
    public string HabitatCategory { get; init; } = string.Empty;

    /// <summary>
    /// Display and navigation order.
    /// </summary>
    public int SortOrder { get; init; }

    /// <summary>
    /// Bilingual display names.
    /// </summary>
    public AquariumText Names { get; init; } = new();

    /// <summary>
    /// Bilingual habitat description.
    /// </summary>
    public AquariumText Habitat { get; init; } = new();

    /// <summary>
    /// Bilingual diet description.
    /// </summary>
    public AquariumText Diet { get; init; } = new();

    /// <summary>
    /// Bilingual discovery or common distribution text.
    /// </summary>
    public AquariumText DiscoveryLocations { get; init; } = new();

    /// <summary>
    /// Bilingual child-friendly summary.
    /// </summary>
    public AquariumText Summary { get; init; } = new();

    /// <summary>
    /// Main profile image.
    /// </summary>
    public AquariumImage MainImage { get; init; } = new();

    /// <summary>
    /// Short story attached to this profile.
    /// </summary>
    public AquariumStory Story { get; init; } = new();

    /// <summary>
    /// Bilingual keywords used for local search.
    /// </summary>
    public List<AquariumText> SearchKeywords { get; init; } = [];
}
