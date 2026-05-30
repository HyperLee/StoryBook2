using System.Net;

namespace StoryBook.Tests.Integration;

public sealed class JourneyPagesTests : IClassFixture<JourneyPageTestFixture>
{
    private readonly JourneyPageTestFixture _fixture;

    public JourneyPagesTests(JourneyPageTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Journeys_page_renders_available_list_default_zhTW_and_no_theme_selector()
    {
        string html = await _fixture.GetOkHtmlAsync("/journeys");

        Assert.Contains("學習旅程", html);
        Assert.Contains("data-journeys-list", html);
        Assert.True(JourneyPageTestFixture.CountOccurrences(html, "data-journey-card") >= 3);
        Assert.Contains("data-journey-slug=\"gentle-giants\"", html);
        Assert.Contains("溫柔巨人小路", html);
        Assert.Contains("建議閱讀時間", html);
        Assert.Contains("建議年齡", html);
        Assert.True(JourneyPageTestFixture.HasLinkTo(html, "/journeys/gentle-giants"));
        Assert.DoesNotContain("data-theme-selector", html, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Journeys_page_returns_ok_with_html_content_type()
    {
        using HttpResponseMessage response = await _fixture.Client.GetAsync("/journeys");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/html", response.Content.Headers.ContentType?.MediaType);
        Assert.Equal("utf-8", response.Content.Headers.ContentType?.CharSet);
    }

    [Fact]
    public async Task Home_and_explore_pages_expose_normal_journey_anchor_links_without_removing_existing_actions()
    {
        string homeHtml = await _fixture.GetOkHtmlAsync("/");
        string exploreHtml = await _fixture.GetOkHtmlAsync("/explore");

        Assert.Contains("學習旅程", homeHtml);
        Assert.Contains("Learning journeys", homeHtml);
        Assert.Contains("學習旅程", exploreHtml);
        Assert.Contains("Learning journeys", exploreHtml);
        Assert.True(JourneyPageTestFixture.HasLinkTo(homeHtml, "/journeys"));
        Assert.True(JourneyPageTestFixture.HasLinkTo(exploreHtml, "/journeys"));
        Assert.True(JourneyPageTestFixture.HasLinkTo(homeHtml, "/explore"));
        Assert.True(JourneyPageTestFixture.HasLinkTo(homeHtml, "/compare"));
        Assert.True(JourneyPageTestFixture.HasLinkTo(exploreHtml, "/compare"));
        Assert.DoesNotContain("data-theme-selector", exploreHtml, StringComparison.OrdinalIgnoreCase);
    }
}
