using StoryBook.Models;

namespace StoryBook.Services;

/// <summary>
/// Validates local learning journey catalog content before it is displayed.
/// </summary>
public sealed class LearningJourneyContentValidator
{
    /// <summary>
    /// Validates the complete journey catalog.
    /// </summary>
    public LearningJourneyContentValidationResult ValidateCatalog(JourneyCatalog? catalog)
    {
        List<string> errors = [];

        if (catalog is null)
        {
            errors.Add("Journey catalog is missing.");
        }

        return new LearningJourneyContentValidationResult(errors);
    }
}
