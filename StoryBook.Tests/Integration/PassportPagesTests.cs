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

    private static bool HasPassportStylesheet(string html)
    {
        return html.Contains("/css/passport.css", StringComparison.OrdinalIgnoreCase)
            || html.Contains("/css/passport.", StringComparison.OrdinalIgnoreCase);
    }
}
