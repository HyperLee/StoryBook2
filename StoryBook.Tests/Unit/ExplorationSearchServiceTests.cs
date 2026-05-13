using StoryBook.Models;
using StoryBook.Services;

namespace StoryBook.Tests.Unit;

public sealed class ExplorationSearchServiceTests
{
    [Theory]
    [InlineData(" 暴，龍! ", "暴龍")]
    [InlineData("SHARK", "shark")]
    [InlineData("Ｓｈａｒｋ！！", "shark")]
    [InlineData("海 水", "海水")]
    public void NormalizeQuery_uses_nfkc_lowercase_and_letters_or_digits_only(string rawQuery, string expected)
    {
        ExplorationSearchService service = new();

        Assert.Equal(expected, service.NormalizeQuery(rawQuery));
    }

    [Fact]
    public void Search_matches_bilingual_text_and_preserves_source_order()
    {
        ExplorationSearchService service = new();
        IReadOnlyList<ExplorationItem> items = CreateItems();

        ExplorationSearchResult chinese = service.Search(items, "暴 龍", null, LanguageCode.ZhTW);
        ExplorationSearchResult english = service.Search(items, "SHARK", null, LanguageCode.ZhTW);
        ExplorationSearchResult plantEater = service.Search(items, "草食", null, LanguageCode.En);

        Assert.Equal("dinosaurs:tyrannosaurus-rex", Assert.Single(chinese.Items).StableId);
        Assert.Equal("aquarium:shark", Assert.Single(english.Items).StableId);
        Assert.Equal(
            ["dinosaurs:triceratops", "dinosaurs:stegosaurus"],
            plantEater.Items.Select(item => item.StableId).ToArray());
        Assert.Equal(ExplorationResultMode.Search, chinese.State.ResultMode);
    }

    [Theory]
    [InlineData("a")]
    [InlineData("海")]
    [InlineData("!!!")]
    public void Search_reports_too_short_and_keeps_collection_visible(string rawQuery)
    {
        ExplorationSearchService service = new();
        IReadOnlyList<ExplorationItem> items = CreateItems();

        ExplorationSearchResult result = service.Search(items, rawQuery, null, LanguageCode.ZhTW);

        Assert.Equal(items.Select(item => item.StableId), result.Items.Select(item => item.StableId));
        Assert.Equal(ExplorationResultMode.TooShort, result.State.ResultMode);
        Assert.True(result.State.VisibleResultCount == items.Count);
    }

    [Fact]
    public void Search_reports_no_results_for_valid_unmatched_query()
    {
        ExplorationSearchService service = new();

        ExplorationSearchResult result = service.Search(CreateItems(), "不存在的故事", null, LanguageCode.ZhTW);

        Assert.Empty(result.Items);
        Assert.Equal(ExplorationResultMode.NoResults, result.State.ResultMode);
    }

    [Fact]
    public void SelectFacet_keeps_one_value_per_group_and_clear_filters_removes_all()
    {
        ExplorationSearchService service = new();

        IReadOnlyDictionary<string, string> selected = service.SelectFacet(
            new Dictionary<string, string>(),
            "source",
            "dinosaurs");
        selected = service.SelectFacet(selected, "source", "aquarium");
        selected = service.SelectFacet(selected, "living-area", "saltwater");
        IReadOnlyDictionary<string, string> cleared = service.ClearFacets(selected);

        Assert.Equal("aquarium", selected["source"]);
        Assert.Equal("saltwater", selected["living-area"]);
        Assert.Equal(2, selected.Count);
        Assert.Empty(cleared);
    }

    [Fact]
    public void Search_applies_facets_with_and_behavior_and_stable_order()
    {
        ExplorationSearchService service = new();
        IReadOnlyList<ExplorationItem> items = CreateItems();
        Dictionary<string, string> selectedFacets = new()
        {
            ["source"] = "dinosaurs",
            ["diet"] = "herbivore",
            ["period"] = "late-cretaceous"
        };

        ExplorationSearchResult result = service.Search(items, null, selectedFacets, LanguageCode.ZhTW);

        ExplorationItem item = Assert.Single(result.Items);
        Assert.Equal("dinosaurs:triceratops", item.StableId);
        Assert.Equal(ExplorationResultMode.Filter, result.State.ResultMode);
    }

    [Fact]
    public void Search_intersects_valid_query_and_selected_filters()
    {
        ExplorationSearchService service = new();
        IReadOnlyList<ExplorationItem> items = CreateItems();
        Dictionary<string, string> selectedFacets = new()
        {
            ["source"] = "aquarium",
            ["living-area"] = "saltwater"
        };

        ExplorationSearchResult result = service.Search(items, "shark", selectedFacets, LanguageCode.En);

        ExplorationItem item = Assert.Single(result.Items);
        Assert.Equal("aquarium:shark", item.StableId);
        Assert.Equal(ExplorationResultMode.Intersection, result.State.ResultMode);
    }

    private static IReadOnlyList<ExplorationItem> CreateItems()
    {
        return
        [
            CreateItem(
                "dinosaurs:tyrannosaurus-rex",
                ExplorationSourceType.Dinosaurs,
                1,
                "暴龍 Tyrannosaurus Rex 肉食 Carnivore 白堊紀 Late Cretaceous 北美洲 North America",
                [
                    Facet("source", "dinosaurs"),
                    Facet("diet", "carnivore"),
                    Facet("period", "late-cretaceous"),
                    Facet("discovery-location", "north-america")
                ]),
            CreateItem(
                "dinosaurs:triceratops",
                ExplorationSourceType.Dinosaurs,
                2,
                "三角龍 Triceratops 草食 Herbivore 白堊紀 Late Cretaceous 北美洲 North America",
                [
                    Facet("source", "dinosaurs"),
                    Facet("diet", "herbivore"),
                    Facet("period", "late-cretaceous"),
                    Facet("discovery-location", "north-america")
                ]),
            CreateItem(
                "dinosaurs:stegosaurus",
                ExplorationSourceType.Dinosaurs,
                3,
                "劍龍 Stegosaurus 草食 Herbivore 侏羅紀 Jurassic 北美洲 North America",
                [
                    Facet("source", "dinosaurs"),
                    Facet("diet", "herbivore"),
                    Facet("period", "late-jurassic"),
                    Facet("discovery-location", "north-america")
                ]),
            CreateItem(
                "aquarium:shark",
                ExplorationSourceType.Aquarium,
                1,
                "鯊魚 Shark 海水 Saltwater 肉食 predator",
                [
                    Facet("source", "aquarium"),
                    Facet("living-area", "saltwater"),
                    Facet("diet", "predator")
                ]),
            CreateItem(
                "aquarium:clownfish",
                ExplorationSourceType.Aquarium,
                2,
                "小丑魚 Clownfish 珊瑚礁 Coral reef 海水 Saltwater",
                [
                    Facet("source", "aquarium"),
                    Facet("living-area", "coral-reef")
                ])
        ];
    }

    private static ExplorationItem CreateItem(
        string stableId,
        ExplorationSourceType source,
        int sortOrder,
        string searchText,
        IReadOnlyList<ExplorationFacetValue> facets)
    {
        return new ExplorationItem
        {
            StableId = stableId,
            Source = source,
            Slug = stableId[(stableId.IndexOf(':') + 1)..],
            DetailHref = $"/{source.GetRoutePrefix()}/{stableId[(stableId.IndexOf(':') + 1)..]}",
            SortOrder = sortOrder,
            SourceLabelZhTW = source.GetLabel(LanguageCode.ZhTW),
            SourceLabelEn = source.GetLabel(LanguageCode.En),
            NameZhTW = stableId,
            NameEn = stableId,
            SummaryZhTW = stableId,
            SummaryEn = stableId,
            ImagePath = "/images/test.png",
            ImageAltTextZhTW = stableId,
            ImageAltTextEn = stableId,
            SearchText = searchText,
            Facets = facets
        };
    }

    private static ExplorationFacetValue Facet(string groupCode, string valueCode)
    {
        return new ExplorationFacetValue
        {
            GroupCode = groupCode,
            ValueCode = valueCode,
            LabelZhTW = valueCode,
            LabelEn = valueCode,
            SortOrder = 1
        };
    }
}
