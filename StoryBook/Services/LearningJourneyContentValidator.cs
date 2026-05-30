using System.Text.RegularExpressions;
using StoryBook.Models;

namespace StoryBook.Services;

/// <summary>
/// Validates local learning journey catalog content before it is displayed.
/// </summary>
public sealed partial class LearningJourneyContentValidator
{
    private static readonly HashSet<string> AllowedSources = ["dinosaurs", "aquarium"];

    /// <summary>
    /// Validates the complete journey catalog.
    /// </summary>
    public LearningJourneyContentValidationResult ValidateCatalog(JourneyCatalog? catalog)
    {
        List<string> errors = [];

        if (catalog is null)
        {
            errors.Add("Journey catalog is missing.");
            return new LearningJourneyContentValidationResult(errors);
        }

        if (catalog.Journeys.Count == 0)
        {
            errors.Add("Journey catalog needs at least one journey.");
        }

        ValidateUniqueValues(catalog.Journeys, journey => journey.Slug, "slug", errors);

        foreach (LearningJourney journey in catalog.Journeys)
        {
            ValidateJourney(journey, errors);
        }

        return new LearningJourneyContentValidationResult(errors);
    }

    private static void ValidateJourney(LearningJourney journey, List<string> errors)
    {
        string slug = string.IsNullOrWhiteSpace(journey.Slug) ? "journey" : journey.Slug;

        if (!SlugRegex().IsMatch(journey.Slug))
        {
            errors.Add($"{slug}: slug must be lowercase kebab-case.");
        }

        if (journey.SortOrder < 1)
        {
            errors.Add($"{slug}: sortOrder must be positive.");
        }

        ValidateText(slug, "title", journey.Title, errors);
        ValidateText(slug, "summary", journey.Summary, errors);
        ValidateText(slug, "ageGuidance", journey.AgeGuidance, errors);

        if (journey.LearningGoals.Count is < 1 or > 3)
        {
            errors.Add($"{slug}: learningGoals must contain 1-3 items.");
        }

        foreach (JourneyText goal in journey.LearningGoals)
        {
            ValidateText(slug, "learningGoals", goal, errors);
        }

        if (journey.SuggestedReadingMinutes < 1)
        {
            errors.Add($"{slug}: suggestedReadingMinutes must be positive.");
        }

        if (journey.StoryReferences.Count == 0)
        {
            errors.Add($"{slug}: storyReferences must not be empty.");
        }

        foreach (JourneyStoryReference reference in journey.StoryReferences)
        {
            ValidateReference(slug, reference, errors);
        }
    }

    private static void ValidateReference(string journeySlug, JourneyStoryReference reference, List<string> errors)
    {
        string sourceCode = reference.Source.Trim().ToLowerInvariant();

        if (!AllowedSources.Contains(sourceCode))
        {
            errors.Add($"{journeySlug}: storyReferences source is not allowed.");
        }

        if (!SlugRegex().IsMatch(reference.Slug))
        {
            errors.Add($"{journeySlug}: storyReferences slug must be lowercase kebab-case.");
        }

        if (reference.SortOrder < 1)
        {
            errors.Add($"{journeySlug}: storyReferences sortOrder must be positive.");
        }
    }

    private static void ValidateText(string slug, string field, JourneyText text, List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(text.Get(LanguageCode.ZhTW)))
        {
            errors.Add($"{slug}: {field}.zhTW is required.");
        }
    }

    private static void ValidateUniqueValues<T>(
        IReadOnlyList<LearningJourney> journeys,
        Func<LearningJourney, T> selector,
        string field,
        List<string> errors)
        where T : notnull
    {
        HashSet<T> values = [];

        foreach (LearningJourney journey in journeys)
        {
            T value = selector(journey);

            if (!values.Add(value))
            {
                errors.Add($"{journey.Slug}: duplicate {field}.");
            }
        }
    }

    [GeneratedRegex("^[a-z0-9]+(?:-[a-z0-9]+)*$")]
    private static partial Regex SlugRegex();
}
