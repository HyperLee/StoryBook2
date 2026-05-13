using System.Globalization;
using System.Text;
using StoryBook.Models;

namespace StoryBook.Services;

/// <summary>
/// Applies page-local search and facet filtering rules for exploration items.
/// </summary>
public sealed class ExplorationSearchService
{
    /// <summary>
    /// Normalizes search text with NFKC, invariant lowercase, and Unicode letter/digit filtering.
    /// </summary>
    public string NormalizeQuery(string? query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return string.Empty;
        }

        string normalized = query.Normalize(NormalizationForm.FormKC).ToLower(CultureInfo.InvariantCulture);
        StringBuilder builder = new(normalized.Length);

        foreach (char character in normalized)
        {
            if (char.IsLetterOrDigit(character))
            {
                builder.Append(character);
            }
        }

        return builder.ToString();
    }

    /// <summary>
    /// Searches and filters items while preserving source and catalog order.
    /// </summary>
    public ExplorationSearchResult Search(
        IReadOnlyList<ExplorationItem> items,
        string? rawQuery,
        IReadOnlyDictionary<string, string>? selectedFacetValues,
        LanguageCode language)
    {
        string normalizedQuery = NormalizeQuery(rawQuery);
        bool hasRawQuery = !string.IsNullOrWhiteSpace(rawQuery);
        bool isTooShort = hasRawQuery && normalizedQuery.Length < 2;
        IReadOnlyDictionary<string, string> selectedFacets = NormalizeSelectedFacets(selectedFacetValues);
        bool hasFilters = selectedFacets.Count > 0;

        IReadOnlyList<ExplorationItem> orderedItems = OrderItems(items);
        IReadOnlyList<ExplorationItem> filteredItems = ApplyFacets(orderedItems, selectedFacets);
        IReadOnlyList<ExplorationItem> visibleItems;

        if (isTooShort)
        {
            visibleItems = filteredItems;
        }
        else if (normalizedQuery.Length > 0)
        {
            visibleItems = filteredItems
                .Where(item => NormalizeQuery(item.SearchText).Contains(normalizedQuery, StringComparison.Ordinal))
                .ToList();
        }
        else
        {
            visibleItems = filteredItems;
        }

        ExplorationResultMode mode = ResolveResultMode(
            normalizedQuery,
            isTooShort,
            hasFilters,
            visibleItems.Count);

        return new ExplorationSearchResult
        {
            Items = visibleItems,
            State = new ExplorationSearchState
            {
                RawQuery = rawQuery ?? string.Empty,
                NormalizedQuery = normalizedQuery,
                SelectedFacetValues = selectedFacets,
                Language = language,
                ResultMode = mode,
                VisibleResultCount = visibleItems.Count
            }
        };
    }

    /// <summary>
    /// Selects one value for a facet group, replacing any previous value in the same group.
    /// </summary>
    public IReadOnlyDictionary<string, string> SelectFacet(
        IReadOnlyDictionary<string, string> selectedFacetValues,
        string groupCode,
        string valueCode)
    {
        Dictionary<string, string> next = new(selectedFacetValues, StringComparer.Ordinal);

        if (string.IsNullOrWhiteSpace(groupCode))
        {
            return next;
        }

        if (string.IsNullOrWhiteSpace(valueCode))
        {
            next.Remove(groupCode);
        }
        else
        {
            next[groupCode] = valueCode;
        }

        return next;
    }

    /// <summary>
    /// Clears all selected facet values.
    /// </summary>
    public IReadOnlyDictionary<string, string> ClearFacets(IReadOnlyDictionary<string, string> selectedFacetValues)
    {
        return new Dictionary<string, string>();
    }

    private static ExplorationResultMode ResolveResultMode(
        string normalizedQuery,
        bool isTooShort,
        bool hasFilters,
        int visibleCount)
    {
        if (isTooShort)
        {
            return ExplorationResultMode.TooShort;
        }

        bool hasSearch = normalizedQuery.Length > 0;

        if (visibleCount == 0 && (hasSearch || hasFilters))
        {
            return ExplorationResultMode.NoResults;
        }

        if (hasSearch && hasFilters)
        {
            return ExplorationResultMode.Intersection;
        }

        if (hasSearch)
        {
            return ExplorationResultMode.Search;
        }

        return hasFilters ? ExplorationResultMode.Filter : ExplorationResultMode.All;
    }

    private static IReadOnlyList<ExplorationItem> ApplyFacets(
        IReadOnlyList<ExplorationItem> items,
        IReadOnlyDictionary<string, string> selectedFacetValues)
    {
        if (selectedFacetValues.Count == 0)
        {
            return items;
        }

        return items
            .Where(item => selectedFacetValues.All(selected =>
                item.Facets.Any(facet =>
                    string.Equals(facet.GroupCode, selected.Key, StringComparison.Ordinal)
                    && string.Equals(facet.ValueCode, selected.Value, StringComparison.Ordinal))))
            .ToList();
    }

    private static IReadOnlyList<ExplorationItem> OrderItems(IReadOnlyList<ExplorationItem> items)
    {
        return items
            .OrderBy(item => item.Source.GetSortOrder())
            .ThenBy(item => item.SortOrder)
            .ToList();
    }

    private static IReadOnlyDictionary<string, string> NormalizeSelectedFacets(
        IReadOnlyDictionary<string, string>? selectedFacetValues)
    {
        if (selectedFacetValues is null || selectedFacetValues.Count == 0)
        {
            return new Dictionary<string, string>();
        }

        return selectedFacetValues
            .Where(pair => !string.IsNullOrWhiteSpace(pair.Key) && !string.IsNullOrWhiteSpace(pair.Value))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
    }
}
