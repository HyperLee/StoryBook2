namespace StoryBook.Models;

/// <summary>
/// Immutable snapshot of passport stories, fixed badges, and source statuses for <c>/passport</c>.
/// </summary>
public sealed class PassportCatalogSnapshot
{
    /// <summary>
    /// Creates a passport snapshot and computes availability flags from the supplied data.
    /// </summary>
    public PassportCatalogSnapshot(
        IReadOnlyList<PassportStoryItem> stories,
        IReadOnlyList<PassportBadgeDefinition> badges,
        IReadOnlyList<PassportSourceStatus> sourceStatuses)
    {
        Stories = stories;
        Badges = badges;
        SourceStatuses = sourceStatuses;
        TotalStoryCount = stories.Count;
        HasAnyStory = stories.Count > 0;
        HasAllSourcesFailed = sourceStatuses.Count > 0 && sourceStatuses.All(status => !status.IsAvailable);
        HasPartialSourceFailure = sourceStatuses.Any(status => !status.IsAvailable) && !HasAllSourcesFailed;
    }

    /// <summary>
    /// Current stories available to the reading passport.
    /// </summary>
    public IReadOnlyList<PassportStoryItem> Stories { get; }

    /// <summary>
    /// Fixed badge definitions.
    /// </summary>
    public IReadOnlyList<PassportBadgeDefinition> Badges { get; }

    /// <summary>
    /// Statuses for each configured storybook source.
    /// </summary>
    public IReadOnlyList<PassportSourceStatus> SourceStatuses { get; }

    /// <summary>
    /// Count of current passport stories.
    /// </summary>
    public int TotalStoryCount { get; }

    /// <summary>
    /// Whether at least one passport story is available.
    /// </summary>
    public bool HasAnyStory { get; }

    /// <summary>
    /// Whether one source failed while at least one other source was still available.
    /// </summary>
    public bool HasPartialSourceFailure { get; }

    /// <summary>
    /// Whether every configured source failed.
    /// </summary>
    public bool HasAllSourcesFailed { get; }
}
