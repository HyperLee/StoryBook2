using System.Net;

namespace StoryBook.Tests.Integration;

public sealed class ComparePagesTests : IClassFixture<ComparePageTestFixture>
{
    private readonly ComparePageTestFixture _fixture;

    public ComparePagesTests(ComparePageTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Compare_page_renders_default_shell_zhTW_controls_status_and_no_theme_selector()
    {
        string html = await _fixture.GetOkHtmlAsync("/compare");

        Assert.Contains("比較故事朋友", html);
        Assert.Contains("第一位故事朋友", html);
        Assert.Contains("第二位故事朋友", html);
        Assert.Contains("清除選擇", html);
        Assert.Contains("data-compare-first-select", html);
        Assert.Contains("data-compare-second-select", html);
        Assert.Contains("data-compare-clear-selection", html);
        Assert.Contains("data-compare-status", html);
        Assert.Contains("aria-live=\"polite\"", html);
        Assert.DoesNotContain("data-theme-selector", html, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Home_and_explore_pages_expose_normal_compare_anchor_links()
    {
        string homeHtml = await _fixture.GetOkHtmlAsync("/");
        string exploreHtml = await _fixture.GetOkHtmlAsync("/explore");

        Assert.Contains("比較故事朋友", homeHtml);
        Assert.Contains("Compare story friends", homeHtml);
        Assert.Contains("比較故事朋友", exploreHtml);
        Assert.Contains("Compare story friends", exploreHtml);
        Assert.True(ComparePageTestFixture.HasLinkTo(homeHtml, "/compare"));
        Assert.True(ComparePageTestFixture.HasLinkTo(exploreHtml, "/compare"));
    }

    [Fact]
    public async Task Compare_page_renders_candidate_metadata_table_rows_detail_links_and_hidden_initial_state()
    {
        string html = await _fixture.GetOkHtmlAsync("/compare");

        Assert.Equal(23, ComparePageTestFixture.CountOccurrences(html, "data-compare-candidate"));
        Assert.Contains("value=\"dinosaurs:tyrannosaurus-rex\"", html);
        Assert.Contains("value=\"aquarium:shark\"", html);
        Assert.Contains("data-compare-source=\"dinosaurs\"", html);
        Assert.Contains("data-compare-source=\"aquarium\"", html);
        Assert.Contains("data-name-zh-tw=\"暴龍\"", html);
        Assert.Contains("data-name-en=\"Tyrannosaurus Rex\"", html);
        Assert.Contains("data-summary-zh-tw=", html);
        Assert.Contains("data-diet-zh-tw=", html);
        Assert.Contains("data-living-area-zh-tw=", html);
        Assert.Contains("data-period-zh-tw=", html);
        Assert.Contains("data-discovery-location-zh-tw=", html);
        Assert.Contains("data-detail-href=\"/dinosaurs/tyrannosaurus-rex\"", html);
        Assert.Contains("data-detail-href=\"/aquarium/shark\"", html);
        Assert.True(ComparePageTestFixture.HasLinkTo(html, "/dinosaurs/tyrannosaurus-rex"));
        Assert.True(ComparePageTestFixture.HasLinkTo(html, "/aquarium/shark"));
        Assert.Contains("data-compare-table", html);
        Assert.Contains("hidden", html);

        foreach (string fieldCode in new[]
        {
            "source",
            "name",
            "diet",
            "living-area",
            "period",
            "discovery-location",
            "summary",
            "detail-link"
        })
        {
            Assert.Contains($"data-compare-field=\"{fieldCode}\"", html);
        }
    }

    [Fact]
    public async Task Compare_page_renders_bilingual_theme_accessibility_and_preserve_state_contract()
    {
        string html = await _fixture.GetOkHtmlAsync("/compare");

        Assert.Contains("data-i18n-zh-tw=\"比較故事朋友\"", html);
        Assert.Contains("data-i18n-en=\"Compare story friends\"", html);
        Assert.Contains("data-aria-label-zh-tw=\"選擇第一位故事朋友\"", html);
        Assert.Contains("data-aria-label-en=\"Choose the first story friend\"", html);
        Assert.Contains("data-aria-label-zh-tw=\"選擇第二位故事朋友\"", html);
        Assert.Contains("data-aria-label-en=\"Choose the second story friend\"", html);
        Assert.Contains("data-compare-preserve-state-on-theme-change=\"true\"", html);
        Assert.Contains("data-storybook-theme-boot", html);
        Assert.Contains("data-storybook-theme-mode", html);
        Assert.Contains("data-storybook-effective-theme", html);
        Assert.DoesNotContain("data-theme-selector", html, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Compare_page_renders_partial_source_failure_with_available_content()
    {
        using HttpClient client = _fixture.CreateClientWithCatalogPaths(aquariumContentPath: "Data/not-found-aquarium.json");
        using HttpResponseMessage response = await client.GetAsync("/compare");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        string html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());

        Assert.Contains("data-compare-partial-failure", html);
        Assert.Contains("有一些故事朋友暫時躲起來了", html);
        Assert.Contains("暴龍", html);
        Assert.DoesNotContain("小丑魚", html);
        Assert.DoesNotContain("not-found-aquarium.json", html);
        Assert.True(ComparePageTestFixture.HasLinkTo(html, "/dinosaurs/tyrannosaurus-rex"));
    }

    [Fact]
    public async Task Compare_page_renders_not_enough_candidates_state_without_misleading_table()
    {
        using HttpClient client = _fixture.CreateClientWithCandidateLimit(1);
        using HttpResponseMessage response = await client.GetAsync("/compare");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        string html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());

        Assert.Contains("data-compare-not-enough", html);
        Assert.Contains("需要至少兩位故事朋友才能比較", html);
        Assert.Contains("data-compare-failure-update-within-ms=\"1000\"", html);
        Assert.DoesNotContain("System.IO", html);
        Assert.DoesNotContain("undefined", html, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Compare_page_renders_all_failed_friendly_state_without_internal_details()
    {
        using HttpClient client = _fixture.CreateClientWithCatalogPaths(
            dinosaurContentPath: "Data/not-found-dinosaurs.json",
            aquariumContentPath: "Data/not-found-aquarium.json");
        using HttpResponseMessage response = await client.GetAsync("/compare");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        string html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());

        Assert.Contains("data-compare-all-failed", html);
        Assert.Contains("故事資料暫時找不到", html);
        Assert.Contains("data-compare-failure-update-within-ms=\"1000\"", html);
        Assert.DoesNotContain("System.IO", html);
        Assert.DoesNotContain("not-found-dinosaurs.json", html);
        Assert.DoesNotContain("not-found-aquarium.json", html);
        Assert.DoesNotContain("null", html, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("undefined", html, StringComparison.OrdinalIgnoreCase);
        Assert.True(ComparePageTestFixture.HasLinkTo(html, "/"));
    }
}
