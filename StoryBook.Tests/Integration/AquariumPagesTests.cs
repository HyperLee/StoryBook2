using System.Net;

namespace StoryBook.Tests.Integration;

public sealed class AquariumPagesTests : IClassFixture<AquariumPageTestFixture>
{
    private readonly AquariumPageTestFixture _fixture;

    public AquariumPagesTests(AquariumPageTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Home_page_exposes_aquarium_storybook_entry_link()
    {
        string html = await _fixture.GetOkHtmlAsync("/");

        Assert.Contains("水族館動物介紹", html);
        Assert.True(AquariumPageTestFixture.HasLinkTo(html, "/aquarium"));
    }

    [Fact]
    public async Task Aquarium_home_displays_cover_welcome_search_language_start_and_directory_shell()
    {
        string html = await _fixture.GetOkHtmlAsync("/aquarium");

        Assert.Contains("水族館故事書", html);
        Assert.Contains("跟著海浪翻開故事書，一次認識一位水族館朋友。", html);
        Assert.Contains("aria-label=\"搜尋水族館動物\"", html);
        Assert.Contains("data-aquarium-search-input", html);
        Assert.Contains("data-language-storage-key=\"storybook.language\"", html);
        Assert.Contains("data-language-option=\"zh-TW\"", html);
        Assert.Contains("data-language-option=\"en\"", html);
        Assert.True(AquariumPageTestFixture.HasLinkTo(html, "/aquarium/clownfish"));
        Assert.Contains("小丑魚", html);
        Assert.Contains("六角恐龍", html);
        Assert.Contains("data-aquarium-search-item", html);
    }

    [Fact]
    public async Task Aquarium_home_data_load_failure_displays_retry_and_home_actions()
    {
        using HttpClient client = _fixture.CreateClientWithCatalogPath("Data/not-found-aquarium.json");
        using HttpResponseMessage response = await client.GetAsync("/aquarium");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        string html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());

        Assert.Contains("水族館資料暫時游不出來", html);
        Assert.True(AquariumPageTestFixture.HasLinkTo(html, "/aquarium"));
        Assert.True(AquariumPageTestFixture.HasLinkTo(html, "/"));
    }
}
