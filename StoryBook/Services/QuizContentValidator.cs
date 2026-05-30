using StoryBook.Models;

namespace StoryBook.Services;

/// <summary>
/// Validates the local quiz catalog before questions are projected to the UI.
/// </summary>
public sealed class QuizContentValidator
{
    /// <summary>
    /// Performs root-level quiz catalog validation.
    /// </summary>
    public QuizContentValidationResult ValidateCatalog(QuizCatalog? catalog)
    {
        if (catalog is null)
        {
            return QuizContentValidationResult.InvalidCatalog("catalog-null");
        }

        if (catalog.Version != 1)
        {
            return QuizContentValidationResult.InvalidCatalog("catalog-version");
        }

        if (catalog.Questions is null)
        {
            return QuizContentValidationResult.InvalidCatalog("catalog-questions-null");
        }

        return new QuizContentValidationResult(catalog.Questions, [], isCatalogUsable: true);
    }
}
