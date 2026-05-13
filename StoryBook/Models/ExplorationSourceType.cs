namespace StoryBook.Models;

/// <summary>
/// Identifies the storybook source that contributes an item to the sitewide exploration page.
/// </summary>
public enum ExplorationSourceType
{
    /// <summary>
    /// Dinosaur storybook content.
    /// </summary>
    Dinosaurs,

    /// <summary>
    /// Aquarium storybook content.
    /// </summary>
    Aquarium
}

/// <summary>
/// Provides display and routing metadata for <see cref="ExplorationSourceType" /> values.
/// </summary>
public static class ExplorationSourceTypeExtensions
{
    /// <summary>
    /// Gets the stable lowercase source code used in data attributes and stable ids.
    /// </summary>
    public static string ToCode(this ExplorationSourceType source)
    {
        return source == ExplorationSourceType.Aquarium ? "aquarium" : "dinosaurs";
    }

    /// <summary>
    /// Gets the canonical route segment used before each source slug.
    /// </summary>
    public static string GetRoutePrefix(this ExplorationSourceType source)
    {
        return source == ExplorationSourceType.Aquarium ? "aquarium" : "dinosaurs";
    }

    /// <summary>
    /// Gets the stable source grouping order for exploration results.
    /// </summary>
    public static int GetSortOrder(this ExplorationSourceType source)
    {
        return source == ExplorationSourceType.Aquarium ? 2 : 1;
    }

    /// <summary>
    /// Gets the localized source label with a Traditional Chinese fallback.
    /// </summary>
    public static string GetLabel(this ExplorationSourceType source, LanguageCode language)
    {
        return source switch
        {
            ExplorationSourceType.Aquarium when language == LanguageCode.En => "Aquarium",
            ExplorationSourceType.Aquarium => "水族館",
            _ when language == LanguageCode.En => "Dinosaurs",
            _ => "恐龍"
        };
    }
}
