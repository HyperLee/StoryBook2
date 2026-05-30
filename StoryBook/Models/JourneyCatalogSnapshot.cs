namespace StoryBook.Models;

/// <summary>
/// Immutable snapshot of available journeys, unavailable journey statuses, and source statuses.
/// </summary>
public sealed class JourneyCatalogSnapshot
{
    /// <summary>
    /// Creates a learning journey snapshot and computes aggregate availability flags.
    /// </summary>
    public JourneyCatalogSnapshot(
        IReadOnlyList<LearningJourney> availableJourneys,
        IReadOnlyList<JourneyAvailabilityStatus> unavailableStatuses,
        IReadOnlyList<JourneySourceStatus> sourceStatuses)
    {
        AvailableJourneys = availableJourneys;
        UnavailableStatuses = unavailableStatuses;
        SourceStatuses = sourceStatuses;
        HasAnyAvailableJourney = availableJourneys.Count > 0;
        HasAllSourcesFailed = sourceStatuses.Count > 0 && sourceStatuses.All(status => !status.IsAvailable);
        HasPartialSourceFailure = sourceStatuses.Any(status => !status.IsAvailable) && !HasAllSourcesFailed;
    }

    /// <summary>
    /// Journeys that may appear as selectable cards on <c>/journeys</c>.
    /// </summary>
    public IReadOnlyList<LearningJourney> AvailableJourneys { get; }

    /// <summary>
    /// Statuses for journeys hidden from the selectable list or unavailable by direct route.
    /// </summary>
    public IReadOnlyList<JourneyAvailabilityStatus> UnavailableStatuses { get; }

    /// <summary>
    /// Statuses for configured source storybooks.
    /// </summary>
    public IReadOnlyList<JourneySourceStatus> SourceStatuses { get; }

    /// <summary>
    /// Whether at least one journey can appear in the list.
    /// </summary>
    public bool HasAnyAvailableJourney { get; }

    /// <summary>
    /// Whether one source failed while at least one other source was still available.
    /// </summary>
    public bool HasPartialSourceFailure { get; }

    /// <summary>
    /// Whether every configured source failed.
    /// </summary>
    public bool HasAllSourcesFailed { get; }
}
