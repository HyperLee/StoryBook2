namespace StoryBook.Models;

/// <summary>
/// Non-sensitive maintenance summary for learning journey validation or source resolution issues.
/// </summary>
public sealed class JourneyDiagnosticSummary
{
    /// <summary>
    /// Stable reason code, such as <c>invalid-reference</c> or <c>source-load-failed</c>.
    /// </summary>
    public string ReasonCode { get; init; } = string.Empty;

    /// <summary>
    /// Journey slug related to the issue, when one is available.
    /// </summary>
    public string? JourneySlug { get; init; }

    /// <summary>
    /// Source code related to the issue, when one is available.
    /// </summary>
    public string? SourceCode { get; init; }

    /// <summary>
    /// Referenced source story slug related to the issue, when one is available.
    /// </summary>
    public string? ReferenceSlug { get; init; }

    /// <summary>
    /// Optional count for aggregate diagnostics.
    /// </summary>
    public int? Count { get; init; }
}
