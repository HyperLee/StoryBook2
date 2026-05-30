using System.Net;
using System.Text.Json;
using StoryBook.Models;

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

    [Fact]
    public async Task Journey_detail_page_renders_contract_start_reading_and_back_link()
    {
        string html = await _fixture.GetOkHtmlAsync("/journeys/clever-hunters");

        Assert.Contains("data-journey-detail", html);
        Assert.Contains("data-journey-slug=\"clever-hunters\"", html);
        Assert.Contains("聰明獵手觀察隊", html);
        Assert.Contains("學習目標", html);
        Assert.Contains("建議閱讀時間", html);
        Assert.Contains("建議年齡", html);
        Assert.Contains("data-journey-start-reading", html);
        Assert.True(JourneyPageTestFixture.HasLinkTo(html, "/dinosaurs/tyrannosaurus-rex"));
        Assert.True(JourneyPageTestFixture.HasLinkTo(html, "/journeys"));
        Assert.DoesNotContain("data-theme-selector", html, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Journey_detail_story_items_are_ordered_normal_source_anchors_only()
    {
        string html = await _fixture.GetOkHtmlAsync("/journeys/clever-hunters");

        Assert.Equal(4, JourneyPageTestFixture.CountOccurrences(html, "data-journey-story-item"));
        Assert.Contains("data-journey-story-id=\"dinosaurs:tyrannosaurus-rex\"", html);
        Assert.Contains("data-journey-story-id=\"aquarium:shark\"", html);
        Assert.True(JourneyPageTestFixture.HasLinkTo(html, "/dinosaurs/tyrannosaurus-rex"));
        Assert.True(JourneyPageTestFixture.HasLinkTo(html, "/dinosaurs/velociraptor"));
        Assert.True(JourneyPageTestFixture.HasLinkTo(html, "/aquarium/shark"));
        Assert.True(JourneyPageTestFixture.HasLinkTo(html, "/aquarium/octopus"));
        Assert.DoesNotContain("/journeys/story/", html, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("/journeys/clever-hunters/", html, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Unknown_journey_slug_renders_friendly_not_found_without_internal_details()
    {
        string html = await _fixture.GetOkHtmlAsync("/journeys/unknown-slug");

        Assert.Contains("data-journey-not-found", html);
        Assert.Contains("找不到這條旅程", html);
        Assert.True(JourneyPageTestFixture.HasLinkTo(html, "/journeys"));
        Assert.True(JourneyPageTestFixture.HasLinkTo(html, "/"));
        Assert.DoesNotContain("System.IO", html);
        Assert.DoesNotContain("Data/", html, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Unavailable_journey_is_hidden_from_list_and_direct_detail_has_no_start_link()
    {
        string path = WriteCatalog(
            CreateJourney("ready-trail"),
            CreateJourney("tiny-trail", references:
            [
                Ref("dinosaurs", "tyrannosaurus-rex", 1),
                Ref("aquarium", "shark", 2)
            ]));
        using HttpClient client = _fixture.CreateClientWithCatalogPaths(journeyContentPath: path);

        string listHtml = WebUtility.HtmlDecode(await client.GetStringAsync("/journeys"));
        string detailHtml = WebUtility.HtmlDecode(await client.GetStringAsync("/journeys/tiny-trail"));

        Assert.Contains("data-journey-slug=\"ready-trail\"", listHtml);
        Assert.DoesNotContain("data-journey-slug=\"tiny-trail\"", listHtml);
        Assert.Contains("data-journey-unavailable", detailHtml);
        Assert.Contains("暫時不能出發", detailHtml);
        Assert.DoesNotContain("data-journey-start-reading", detailHtml);
        Assert.DoesNotContain("System.IO", detailHtml);
    }

    [Fact]
    public async Task Partial_source_failure_renders_available_content_and_no_internal_diagnostics()
    {
        string path = WriteCatalog(
            CreateJourney("dino-only-trail", references:
            [
                Ref("dinosaurs", "tyrannosaurus-rex", 1),
                Ref("dinosaurs", "triceratops", 2),
                Ref("dinosaurs", "stegosaurus", 3)
            ]),
            CreateJourney("mixed-trail"));
        using HttpClient client = _fixture.CreateClientWithCatalogPaths(
            journeyContentPath: path,
            aquariumContentPath: "Data/not-found-aquarium.json");
        using HttpResponseMessage response = await client.GetAsync("/journeys");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        string html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());

        Assert.Contains("data-journeys-partial-failure", html);
        Assert.Contains("data-journey-slug=\"dino-only-trail\"", html);
        Assert.DoesNotContain("data-journey-slug=\"mixed-trail\"", html);
        Assert.DoesNotContain("not-found-aquarium.json", html);
        Assert.DoesNotContain("System.IO", html);
    }

    [Fact]
    public async Task All_unavailable_state_renders_home_anchor_without_internal_diagnostics()
    {
        string path = WriteCatalog(CreateJourney("tiny-trail", references:
        [
            Ref("dinosaurs", "tyrannosaurus-rex", 1),
            Ref("aquarium", "shark", 2)
        ]));
        using HttpClient client = _fixture.CreateClientWithCatalogPaths(journeyContentPath: path);
        using HttpResponseMessage response = await client.GetAsync("/journeys");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        string html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());

        Assert.Contains("data-journeys-all-unavailable", html);
        Assert.True(JourneyPageTestFixture.HasLinkTo(html, "/"));
        Assert.DoesNotContain("System.IO", html);
        Assert.DoesNotContain("Data/", html, StringComparison.OrdinalIgnoreCase);
    }

    private static string WriteCatalog(params LearningJourney[] journeys)
    {
        string json = JsonSerializer.Serialize(
            new JourneyCatalog { Journeys = journeys.ToList() },
            new JsonSerializerOptions(JsonSerializerDefaults.Web));
        return JourneyPageTestFixture.WriteJourneyCatalog(json);
    }

    private static LearningJourney CreateJourney(
        string slug,
        List<JourneyStoryReference>? references = null)
    {
        return new LearningJourney
        {
            Slug = slug,
            SortOrder = 1,
            Title = Text("旅程", "Journey"),
            Summary = Text("旅程摘要", "Journey summary"),
            LearningGoals = [Text("學習目標", "Learning goal")],
            SuggestedReadingMinutes = 10,
            AgeGuidance = Text("5-7 歲", "Ages 5-7"),
            StoryReferences = references ??
            [
                Ref("dinosaurs", "tyrannosaurus-rex", 1),
                Ref("dinosaurs", "triceratops", 2),
                Ref("aquarium", "shark", 3)
            ]
        };
    }

    private static JourneyStoryReference Ref(string source, string slug, int sortOrder)
    {
        return new JourneyStoryReference
        {
            Source = source,
            Slug = slug,
            SortOrder = sortOrder
        };
    }

    private static JourneyText Text(string zhTW, string en)
    {
        return new JourneyText
        {
            ZhTW = zhTW,
            En = en
        };
    }
}
