using System.Net;
using StoryBook.Tests.Support;

namespace StoryBook.Tests.Integration;

public sealed class ExplorePagesTests : IClassFixture<ExplorePageTestFixture>
{
    private readonly ExplorePageTestFixture _fixture;

    public ExplorePagesTests(ExplorePageTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Home_page_exposes_explore_entry_link()
    {
        string html = await _fixture.GetOkHtmlAsync("/");

        Assert.Contains("探索全部故事", html);
        Assert.Contains("Explore all stories", html);
        Assert.True(ExplorePageTestFixture.HasLinkTo(html, "/explore"));
    }

    [Fact]
    public async Task Explore_page_renders_full_collection_default_zhTW_sources_and_detail_anchors()
    {
        string html = await _fixture.GetOkHtmlAsync("/explore");

        Assert.Contains("全站探索", html);
        Assert.Contains("恐龍", html);
        Assert.Contains("水族館", html);
        Assert.Contains("暴龍", html);
        Assert.Contains("小丑魚", html);
        Assert.Contains("data-explore-result-item", html);
        Assert.Contains("data-explore-source=\"dinosaurs\"", html);
        Assert.Contains("data-explore-source=\"aquarium\"", html);
        Assert.True(ExplorePageTestFixture.HasLinkTo(html, "/dinosaurs/tyrannosaurus-rex"));
        Assert.True(ExplorePageTestFixture.HasLinkTo(html, "/aquarium/clownfish"));
        Assert.True(ExplorePageTestFixture.HasLinkTo(html, "/"));
    }

    [Fact]
    public async Task Explore_page_renders_partial_source_failure_with_available_content()
    {
        using HttpClient client = _fixture.CreateClientWithCatalogPaths(aquariumContentPath: "Data/not-found-aquarium.json");
        using HttpResponseMessage response = await client.GetAsync("/explore");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        string html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());

        Assert.Contains("data-explore-partial-failure", html);
        Assert.Contains("有一些故事暫時躲起來了", html);
        Assert.Contains("暴龍", html);
        Assert.DoesNotContain("小丑魚", html);
        Assert.True(ExplorePageTestFixture.HasLinkTo(html, "/dinosaurs/tyrannosaurus-rex"));
    }

    [Fact]
    public async Task Explore_page_renders_all_failed_friendly_state_without_blank_page()
    {
        using HttpClient client = _fixture.CreateClientWithCatalogPaths(
            dinosaurContentPath: "Data/not-found-dinosaurs.json",
            aquariumContentPath: "Data/not-found-aquarium.json");
        using HttpResponseMessage response = await client.GetAsync("/explore");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        string html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());

        Assert.Contains("data-explore-all-failed", html);
        Assert.Contains("故事資料暫時找不到", html);
        Assert.DoesNotContain("System.IO", html);
        Assert.DoesNotContain("not-found-dinosaurs.json", html);
        Assert.True(ExplorePageTestFixture.HasLinkTo(html, "/"));
    }

    [Fact]
    public async Task Explore_page_renders_search_contract_without_history_writes()
    {
        string html = await _fixture.GetOkHtmlAsync("/explore");

        Assert.Contains("type=\"search\"", html);
        Assert.Contains("data-explore-search-input", html);
        Assert.Contains("data-explore-clear-search", html);
        Assert.Contains("data-explore-result-status", html);
        Assert.Contains("aria-live=\"polite\"", html);
        Assert.Contains("data-explore-too-short", html);
        Assert.Contains("請輸入至少兩個字", html);
        Assert.Contains("data-explore-no-results", html);
        Assert.Contains("沒有找到符合的故事朋友", html);
        Assert.Contains("data-explore-search-text", html);
        Assert.Contains("暴龍", html);
        Assert.Contains("shark", html, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("pushState", html, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("replaceState", html, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Explore_script_keeps_search_and_filter_state_page_local()
    {
        string scriptPath = Path.Combine(TestPaths.StoryBookRoot, "wwwroot", "js", "explore.js");
        string script = File.ReadAllText(scriptPath);

        Assert.Contains("data-explore-search-input", script);
        Assert.Contains("data-explore-filter-value", script);
        Assert.DoesNotContain("pushState", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("replaceState", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("location.search", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("localStorage.setItem", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("sessionStorage", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("document.cookie", script, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Explore_page_renders_filter_contract_and_result_facet_metadata()
    {
        string html = await _fixture.GetOkHtmlAsync("/explore");

        Assert.Contains("data-explore-filters", html);
        Assert.Contains("data-explore-filter-group=\"source\"", html);
        Assert.Contains("data-explore-filter-group=\"diet\"", html);
        Assert.Contains("data-explore-filter-group=\"living-area\"", html);
        Assert.Contains("data-explore-filter-group=\"period\"", html);
        Assert.Contains("data-explore-filter-group=\"discovery-location\"", html);
        Assert.Contains("data-explore-filter-value=\"dinosaurs\"", html);
        Assert.Contains("data-explore-filter-value=\"aquarium\"", html);
        Assert.Contains("data-explore-clear-filters", html);
        Assert.Contains("data-explore-facets", html);
        Assert.Contains("source:dinosaurs", html);
        Assert.Contains("living-area:coral-reef", html);
    }

    [Fact]
    public async Task Explore_page_renders_bilingual_theme_and_accessibility_contract()
    {
        string html = await _fixture.GetOkHtmlAsync("/explore");

        Assert.Contains("data-i18n-zh-tw=\"全站探索\"", html);
        Assert.Contains("data-i18n-en=\"Explore all stories\"", html);
        Assert.Contains("data-aria-label-zh-tw=\"搜尋全部故事\"", html);
        Assert.Contains("data-aria-label-en=\"Search all stories\"", html);
        Assert.Contains("data-placeholder-zh-tw=\"輸入名稱、食性、生活區域、時期或地點\"", html);
        Assert.Contains("data-placeholder-en=\"Search by name, diet, home, period, or place\"", html);
        Assert.Contains("data-storybook-theme-boot", html);
        Assert.Contains("data-storybook-theme-mode", html);
        Assert.Contains("data-storybook-effective-theme", html);
        Assert.DoesNotContain("data-theme-selector", html, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("data-explore-preserve-state-on-theme-change=\"true\"", html);
    }
}
