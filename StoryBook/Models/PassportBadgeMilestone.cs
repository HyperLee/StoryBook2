namespace StoryBook.Models;

/// <summary>
/// Identifies how a reading passport badge is unlocked from completed story items.
/// </summary>
public enum PassportBadgeMilestone
{
    /// <summary>
    /// Unlocks when the total completed story count reaches a target count.
    /// </summary>
    CompletedCountAtLeast,

    /// <summary>
    /// Unlocks when every available story in one source has been completed.
    /// </summary>
    CompletedAllInSource,

    /// <summary>
    /// Unlocks when every available story across all passport sources has been completed.
    /// </summary>
    CompletedAllStories
}
