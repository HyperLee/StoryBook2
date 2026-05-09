namespace StoryBook.Models;

/// <summary>
/// Represents one browsable prehistoric animal profile.
/// </summary>
public sealed class DinosaurProfile
{
    /// <summary>
    /// Unique lowercase kebab-case slug used by <c>/dinosaurs/{slug}</c>.
    /// </summary>
    public string Slug { get; init; } = string.Empty;

    /// <summary>
    /// Content category. Pteranodon uses <c>prehistoric-flying-reptile</c>.
    /// </summary>
    public string Category { get; init; } = string.Empty;

    /// <summary>
    /// Display and navigation order.
    /// </summary>
    public int SortOrder { get; init; }

    /// <summary>
    /// Bilingual display names.
    /// </summary>
    public DinosaurText Names { get; init; } = new();

    /// <summary>
    /// Bilingual time period text.
    /// </summary>
    public DinosaurText Periods { get; init; } = new();

    /// <summary>
    /// Bilingual diet text.
    /// </summary>
    public DinosaurText Diet { get; init; } = new();

    /// <summary>
    /// Bilingual discovery location text.
    /// </summary>
    public DinosaurText DiscoveryLocations { get; init; } = new();

    /// <summary>
    /// Bilingual size description.
    /// </summary>
    public DinosaurText SizeDescription { get; init; } = new();

    /// <summary>
    /// Bilingual child-friendly summary.
    /// </summary>
    public DinosaurText Summary { get; init; } = new();

    /// <summary>
    /// Bilingual note for prehistoric animals that are not true dinosaurs.
    /// </summary>
    public DinosaurText? NotDinosaurNote { get; init; }

    /// <summary>
    /// Main profile illustration.
    /// </summary>
    public DinosaurIllustration MainImage { get; init; } = new();

    /// <summary>
    /// Short story attached to this profile.
    /// </summary>
    public DinosaurStory Story { get; init; } = new();

    /// <summary>
    /// Bilingual keywords used for local search.
    /// </summary>
    public List<DinosaurText> SearchKeywords { get; init; } = [];
}
