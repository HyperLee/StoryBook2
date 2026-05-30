using System.Net;

namespace StoryBook.Tests.Integration;

public sealed class PassportPagesTests : IClassFixture<PassportPageTestFixture>
{
    private readonly PassportPageTestFixture _fixture;

    public PassportPagesTests(PassportPageTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Dinosaur_detail_renders_passport_completion_region_for_valid_story()
    {
        string html = await _fixture.GetOkHtmlAsync("/dinosaurs/triceratops");

        Assert.Contains("data-passport-story", html);
        Assert.Contains("data-passport-source=\"dinosaurs\"", html);
        Assert.Contains("data-passport-slug=\"triceratops\"", html);
        Assert.Contains("data-passport-complete", html);
        Assert.Contains("aria-label=\"把三角龍蓋到我的探險護照\"", html);
        Assert.Contains("data-aria-label-en=\"Stamp Triceratops in my passport\"", html);
        Assert.Contains("data-passport-status", html);
        Assert.Contains("aria-live=\"polite\"", html);
        Assert.Contains("data-passport-link", html);
        Assert.True(PassportPageTestFixture.HasLinkTo(html, "/passport"));
        Assert.True(HasPassportStylesheet(html));
        Assert.Contains("js/passport.js", html);
    }

    [Fact]
    public async Task Aquarium_detail_renders_passport_completion_region_for_valid_story()
    {
        string html = await _fixture.GetOkHtmlAsync("/aquarium/sea-turtle");

        Assert.Contains("data-passport-story", html);
        Assert.Contains("data-passport-source=\"aquarium\"", html);
        Assert.Contains("data-passport-slug=\"sea-turtle\"", html);
        Assert.Contains("data-passport-complete", html);
        Assert.Contains("aria-label=\"把海龜蓋到我的探險護照\"", html);
        Assert.Contains("data-aria-label-en=\"Stamp Sea turtle in my passport\"", html);
        Assert.Contains("data-passport-status", html);
        Assert.Contains("aria-live=\"polite\"", html);
        Assert.Contains("data-passport-link", html);
        Assert.True(PassportPageTestFixture.HasLinkTo(html, "/passport"));
        Assert.True(HasPassportStylesheet(html));
        Assert.Contains("js/passport.js", html);
    }

    [Fact]
    public async Task Invalid_or_unavailable_detail_pages_do_not_render_completion_control()
    {
        string dinosaurNotFoundHtml = await _fixture.GetHtmlAsync("/dinosaurs/not-a-real-slug", HttpStatusCode.NotFound);
        string aquariumNotFoundHtml = await _fixture.GetHtmlAsync("/aquarium/not-a-real-slug", HttpStatusCode.NotFound);

        using HttpClient aquariumFailureClient = _fixture.CreateClientWithCatalogPaths(aquariumContentPath: "Data/not-found-aquarium.json");
        using HttpResponseMessage aquariumFailureResponse = await aquariumFailureClient.GetAsync("/aquarium/sea-turtle");
        string aquariumFailureHtml = WebUtility.HtmlDecode(await aquariumFailureResponse.Content.ReadAsStringAsync());

        Assert.DoesNotContain("data-passport-complete", dinosaurNotFoundHtml);
        Assert.DoesNotContain("data-passport-story", dinosaurNotFoundHtml);
        Assert.DoesNotContain("data-passport-complete", aquariumNotFoundHtml);
        Assert.DoesNotContain("data-passport-story", aquariumNotFoundHtml);
        Assert.DoesNotContain("data-passport-complete", aquariumFailureHtml);
        Assert.DoesNotContain("data-passport-story", aquariumFailureHtml);
    }

    [Fact]
    public async Task Passport_page_renders_route_dom_contract_badges_story_metadata_and_no_theme_selector()
    {
        string html = await _fixture.GetOkHtmlAsync("/passport");

        Assert.Contains("小小探險護照", html);
        Assert.Contains("data-passport-page", html);
        Assert.Contains("data-passport-storage-key=\"storybook.passport\"", html);
        Assert.Contains("data-passport-version=\"1\"", html);
        Assert.Contains("data-passport-language-storage-key=\"storybook.language\"", html);
        Assert.Contains("data-passport-summary", html);
        Assert.Contains("data-passport-read-list", html);
        Assert.Contains("data-passport-empty", html);
        Assert.Equal(23, PassportPageTestFixture.CountOccurrences(html, "data-passport-story-item"));
        Assert.Equal(5, PassportPageTestFixture.CountOccurrences(html, "data-passport-badge "));
        Assert.Contains("data-passport-story-id=\"dinosaurs:triceratops\"", html);
        Assert.Contains("data-passport-story-id=\"aquarium:sea-turtle\"", html);
        Assert.Contains("data-passport-href=\"/dinosaurs/triceratops\"", html);
        Assert.Contains("data-passport-href=\"/aquarium/sea-turtle\"", html);
        Assert.Contains("data-passport-badge-code=\"all-dinosaurs\"", html);
        Assert.Contains("data-passport-badge-milestone=\"CompletedAllInSource\"", html);
        Assert.True(HasPassportStylesheet(html));
        Assert.Contains("js/passport.js", html);
        Assert.DoesNotContain("data-theme-selector", html, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Home_and_shared_navigation_expose_normal_passport_anchor_links()
    {
        string homeHtml = await _fixture.GetOkHtmlAsync("/");
        string dinosaurHtml = await _fixture.GetOkHtmlAsync("/dinosaurs/triceratops");

        Assert.Contains("我的探險護照", homeHtml);
        Assert.Contains("My adventure passport", homeHtml);
        Assert.Contains("我的探險護照", dinosaurHtml);
        Assert.Contains("My adventure passport", dinosaurHtml);
        Assert.True(PassportPageTestFixture.HasLinkTo(homeHtml, "/passport"));
        Assert.True(PassportPageTestFixture.HasLinkTo(dinosaurHtml, "/passport"));
    }

    [Fact]
    public async Task Passport_page_renders_partial_and_all_source_failure_friendly_states()
    {
        using HttpClient partialClient = _fixture.CreateClientWithCatalogPaths(aquariumContentPath: "Data/not-found-aquarium.json");
        using HttpResponseMessage partialResponse = await partialClient.GetAsync("/passport");
        string partialHtml = WebUtility.HtmlDecode(await partialResponse.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.OK, partialResponse.StatusCode);
        Assert.Contains("data-passport-partial-failure", partialHtml);
        Assert.Contains("有一些故事朋友暫時躲起來了", partialHtml);
        Assert.Contains("暴龍", partialHtml);
        Assert.DoesNotContain("小丑魚", partialHtml);
        Assert.DoesNotContain("not-found-aquarium.json", partialHtml);

        using HttpClient allFailedClient = _fixture.CreateClientWithCatalogPaths(
            dinosaurContentPath: "Data/not-found-dinosaurs.json",
            aquariumContentPath: "Data/not-found-aquarium.json");
        using HttpResponseMessage allFailedResponse = await allFailedClient.GetAsync("/passport");
        string allFailedHtml = WebUtility.HtmlDecode(await allFailedResponse.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.OK, allFailedResponse.StatusCode);
        Assert.Contains("data-passport-all-failed", allFailedHtml);
        Assert.Contains("故事朋友暫時躲起來了", allFailedHtml);
        Assert.DoesNotContain("System.IO", allFailedHtml);
        Assert.DoesNotContain("not-found-dinosaurs.json", allFailedHtml);
        Assert.True(PassportPageTestFixture.HasLinkTo(allFailedHtml, "/"));
    }

    private static bool HasPassportStylesheet(string html)
    {
        return html.Contains("/css/passport.css", StringComparison.OrdinalIgnoreCase)
            || html.Contains("/css/passport.", StringComparison.OrdinalIgnoreCase);
    }
}
