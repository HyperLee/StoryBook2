using System.Net;

namespace StoryBook.Tests.Integration;

public sealed class RoutingAndFallbackTests : IClassFixture<DinosaurPageTestFixture>
{
    private readonly DinosaurPageTestFixture _fixture;

    public RoutingAndFallbackTests(DinosaurPageTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Unknown_dinosaur_slug_returns_friendly_not_found_with_home_and_list_links()
    {
        using HttpResponseMessage response = await _fixture.Client.GetAsync("/dinosaurs/not-a-real-slug");
        string html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Contains("找不到這位史前朋友", html);
        Assert.True(DinosaurPageTestFixture.HasLinkTo(html, "/"));
        Assert.True(DinosaurPageTestFixture.HasLinkTo(html, "/dinosaurs"));
    }

    [Fact]
    public async Task Canonical_dinosaur_routes_are_server_rendered_anchor_routes()
    {
        string listHtml = await _fixture.GetOkHtmlAsync("/dinosaurs");
        string detailHtml = await _fixture.GetOkHtmlAsync("/dinosaurs/tyrannosaurus-rex");

        Assert.True(DinosaurPageTestFixture.HasLinkTo(listHtml, "/dinosaurs/tyrannosaurus-rex"));
        Assert.Contains("/dinosaurs/triceratops", detailHtml);
        Assert.DoesNotContain("pushState", listHtml, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("pushState", detailHtml, StringComparison.OrdinalIgnoreCase);
    }
}
