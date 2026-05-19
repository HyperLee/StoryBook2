namespace StoryBook.Models;

/// <summary>
/// Immutable snapshot of candidates, comparison fields, and source statuses for <c>/compare</c>.
/// </summary>
public sealed class ComparisonCatalogSnapshot
{
    /// <summary>
    /// Creates a comparison snapshot and computes availability flags from the supplied data.
    /// </summary>
    public ComparisonCatalogSnapshot(
        IReadOnlyList<ComparisonCandidate> candidates,
        IReadOnlyList<ComparisonFieldDefinition> fieldDefinitions,
        IReadOnlyList<ComparisonSourceStatus> sourceStatuses)
    {
        Candidates = candidates;
        FieldDefinitions = fieldDefinitions;
        SourceStatuses = sourceStatuses;
        HasEnoughCandidates = candidates.Count >= 2;
        HasAllSourcesFailed = sourceStatuses.Count > 0 && sourceStatuses.All(status => !status.IsAvailable);
        HasPartialFailure = sourceStatuses.Any(status => !status.IsAvailable) && !HasAllSourcesFailed;
    }

    /// <summary>
    /// Current selectable comparison candidates.
    /// </summary>
    public IReadOnlyList<ComparisonCandidate> Candidates { get; }

    /// <summary>
    /// Fixed comparison table field definitions.
    /// </summary>
    public IReadOnlyList<ComparisonFieldDefinition> FieldDefinitions { get; }

    /// <summary>
    /// Statuses for each configured storybook source.
    /// </summary>
    public IReadOnlyList<ComparisonSourceStatus> SourceStatuses { get; }

    /// <summary>
    /// Whether at least two candidates are available.
    /// </summary>
    public bool HasEnoughCandidates { get; }

    /// <summary>
    /// Whether one source failed while at least one other source was still available.
    /// </summary>
    public bool HasPartialFailure { get; }

    /// <summary>
    /// Whether every configured source failed.
    /// </summary>
    public bool HasAllSourcesFailed { get; }
}
