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

    [Theory]
    [InlineData("/aquarium/clownfish", "小丑魚", "珊瑚礁", "clownfish-main.png", "clownfish-story.png")]
    [InlineData("/aquarium/axolotl", "六角恐龍", "淡水", "axolotl-main.png", "axolotl-story.png")]
    public async Task Aquarium_detail_displays_required_profile_fields_for_direct_slug(
        string path,
        string name,
        string category,
        string mainImage,
        string storyImage)
    {
        string html = await _fixture.GetOkHtmlAsync(path);

        Assert.Contains(name, html);
        Assert.Contains(category, html);
        Assert.Contains("生活環境", html);
        Assert.Contains("食性", html);
        Assert.Contains("發現地點", html);
        Assert.Contains("小故事", html);
        Assert.Contains(mainImage, html);
        Assert.Contains(storyImage, html);
        Assert.Contains("data-alt-zh-tw=", html);
        Assert.Contains("data-alt-en=", html);
    }

    [Fact]
    public async Task Aquarium_detail_returns_friendly_404_for_unknown_slug()
    {
        string html = await _fixture.GetHtmlAsync("/aquarium/not-a-real-slug", HttpStatusCode.NotFound);

        Assert.Contains("找不到這位水族館朋友", html);
        Assert.True(AquariumPageTestFixture.HasLinkTo(html, "/"));
        Assert.True(AquariumPageTestFixture.HasLinkTo(html, "/aquarium"));
    }

    [Fact]
    public async Task Aquarium_detail_data_load_failure_displays_retry_and_home_actions()
    {
        using HttpClient client = _fixture.CreateClientWithCatalogPath("Data/not-found-aquarium.json");
        using HttpResponseMessage response = await client.GetAsync("/aquarium/clownfish");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        string html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());

        Assert.Contains("水族館資料暫時游不出來", html);
        Assert.True(AquariumPageTestFixture.HasLinkTo(html, "/aquarium/clownfish"));
        Assert.True(AquariumPageTestFixture.HasLinkTo(html, "/"));
    }

    [Fact]
    public async Task Aquarium_detail_exposes_previous_next_anchor_navigation_and_disabled_boundaries()
    {
        string firstHtml = await _fixture.GetOkHtmlAsync("/aquarium/clownfish");
        Assert.Contains("上一頁", firstHtml);
        Assert.Contains("aria-disabled=\"true\"", firstHtml);
        Assert.True(AquariumPageTestFixture.HasLinkTo(firstHtml, "/aquarium/seahorse"));

        string middleHtml = await _fixture.GetOkHtmlAsync("/aquarium/seahorse");
        Assert.True(AquariumPageTestFixture.HasLinkTo(middleHtml, "/aquarium/clownfish"));
        Assert.True(AquariumPageTestFixture.HasLinkTo(middleHtml, "/aquarium/sea-turtle"));

        string lastHtml = await _fixture.GetOkHtmlAsync("/aquarium/axolotl");
        Assert.True(AquariumPageTestFixture.HasLinkTo(lastHtml, "/aquarium/goldfish"));
        Assert.Contains("下一頁", lastHtml);
        Assert.Contains("aria-disabled=\"true\"", lastHtml);
        Assert.Contains("data-aquarium-rapid-navigation", middleHtml);
    }

    [Fact]
    public async Task Aquarium_home_renders_accessible_search_contract_and_result_cards()
    {
        string html = await _fixture.GetOkHtmlAsync("/aquarium");

        Assert.Contains("aria-label=\"搜尋水族館動物\"", html);
        Assert.Contains("data-aquarium-search-input", html);
        Assert.Contains("data-aquarium-clear-search", html);
        Assert.Contains("data-aquarium-too-short", html);
        Assert.Contains("請輸入至少兩個字", html);
        Assert.Contains("data-aquarium-no-results", html);
        Assert.Contains("沒有找到符合的水族館朋友", html);
        Assert.Contains("data-aquarium-search-updates-within-ms=\"1000\"", html);
        Assert.Contains("data-aquarium-search-item", html);
        Assert.Contains("data-aquarium-search-text", html);
        Assert.Contains("珊瑚礁", html);
        Assert.Contains("小丑魚", html);
        Assert.True(AquariumPageTestFixture.HasLinkTo(html, "/aquarium/seahorse"));
    }

    [Fact]
    public async Task Aquarium_pages_emit_language_switch_controls_bilingual_attributes_and_feature_script()
    {
        string listHtml = await _fixture.GetOkHtmlAsync("/aquarium");
        string detailHtml = await _fixture.GetOkHtmlAsync("/aquarium/clownfish");

        Assert.Contains("data-language-storage-key=\"storybook.language\"", listHtml);
        Assert.Contains("data-language-option=\"zh-TW\"", listHtml);
        Assert.Contains("data-language-option=\"en\"", listHtml);
        Assert.Contains("data-i18n-en=\"Aquarium Storybook\"", listHtml);
        Assert.Contains("data-placeholder-en=\"Search by name, habitat, or diet\"", listHtml);
        Assert.Contains("data-aria-label-en=\"Search aquarium animals\"", listHtml);
        Assert.Contains("js/aquarium.js", listHtml);

        Assert.Contains("data-i18n-en=\"Clownfish\"", detailHtml);
        Assert.Contains("data-i18n-en=\"Coral reef\"", detailHtml);
        Assert.Contains("data-alt-en=\"Cute storybook clownfish", detailHtml);
        Assert.Contains("data-aria-label-en=\"Aquarium navigation\"", detailHtml);
        Assert.Contains("js/aquarium.js", detailHtml);
    }

    [Fact]
    public async Task Aquarium_detail_renders_accessible_image_modal_contract()
    {
        string html = await _fixture.GetOkHtmlAsync("/aquarium/clownfish");

        Assert.Contains("aquarium-image-button", html);
        Assert.Contains("aria-label=\"開啟小丑魚大圖\"", html);
        Assert.Contains("data-aria-label-en=\"Open large image for Clownfish\"", html);
        Assert.Contains("data-aquarium-modal-open=\"aquarium-image-modal\"", html);
        Assert.Contains("id=\"aquarium-image-modal\"", html);
        Assert.Contains("modal fade", html);
        Assert.Contains("aria-label=\"關閉大圖\"", html);
        Assert.Contains("data-aria-label-en=\"Close large image\"", html);
        Assert.Contains("aquarium-modal__image", html);
        Assert.Contains("圖片暫時沒有游出來", html);
    }
}
