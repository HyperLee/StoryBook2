namespace StoryBook.Tests.Integration;

public sealed class DinosaurPagesTests : IClassFixture<DinosaurPageTestFixture>
{
    private readonly DinosaurPageTestFixture _fixture;

    public DinosaurPagesTests(DinosaurPageTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Home_page_exposes_dinosaur_intro_entry_link()
    {
        string html = await _fixture.GetOkHtmlAsync("/");

        Assert.Contains("恐龍介紹", html);
        Assert.True(DinosaurPageTestFixture.HasLinkTo(html, "/dinosaurs"));
    }

    [Fact]
    public async Task Dinosaur_list_displays_first_profile_and_profile_links()
    {
        string html = await _fixture.GetOkHtmlAsync("/dinosaurs");

        Assert.Contains("暴龍", html);
        Assert.Contains("白堊紀晚期", html);
        Assert.Contains("肉食性", html);
        Assert.Contains("北美洲", html);
        Assert.Contains("tyrannosaurus-rex-main.png", html);
        Assert.True(DinosaurPageTestFixture.HasLinkTo(html, "/dinosaurs/tyrannosaurus-rex"));
        Assert.True(DinosaurPageTestFixture.HasLinkTo(html, "/dinosaurs/triceratops"));
    }

    [Fact]
    public async Task Dinosaur_detail_displays_required_profile_fields_for_direct_slug()
    {
        string html = await _fixture.GetOkHtmlAsync("/dinosaurs/tyrannosaurus-rex");

        Assert.Contains("暴龍", html);
        Assert.Contains("白堊紀晚期", html);
        Assert.Contains("肉食性", html);
        Assert.Contains("北美洲", html);
        Assert.Contains("像一輛大巴士", html);
        Assert.Contains("暴龍有強大的咬合力", html);
        Assert.Contains("tyrannosaurus-rex-main.png", html);
        Assert.Contains("可愛繪本風暴龍", html);
    }

    [Fact]
    public async Task Dinosaur_detail_exposes_previous_next_anchor_navigation_and_disabled_boundaries()
    {
        string firstHtml = await _fixture.GetOkHtmlAsync("/dinosaurs/tyrannosaurus-rex");
        Assert.Contains("上一頁", firstHtml);
        Assert.Contains("aria-disabled=\"true\"", firstHtml);
        Assert.True(DinosaurPageTestFixture.HasLinkTo(firstHtml, "/dinosaurs/triceratops"));

        string middleHtml = await _fixture.GetOkHtmlAsync("/dinosaurs/triceratops");
        Assert.True(DinosaurPageTestFixture.HasLinkTo(middleHtml, "/dinosaurs/tyrannosaurus-rex"));
        Assert.True(DinosaurPageTestFixture.HasLinkTo(middleHtml, "/dinosaurs/stegosaurus"));

        string lastHtml = await _fixture.GetOkHtmlAsync("/dinosaurs/parasaurolophus");
        Assert.True(DinosaurPageTestFixture.HasLinkTo(lastHtml, "/dinosaurs/ankylosaurus"));
        Assert.Contains("下一頁", lastHtml);
        Assert.Contains("aria-disabled=\"true\"", lastHtml);
    }

    [Fact]
    public async Task Dinosaur_pages_expose_visible_home_link()
    {
        string listHtml = await _fixture.GetOkHtmlAsync("/dinosaurs");
        string detailHtml = await _fixture.GetOkHtmlAsync("/dinosaurs/tyrannosaurus-rex");

        Assert.Contains("回首頁", listHtml);
        Assert.Contains("回首頁", detailHtml);
        Assert.True(DinosaurPageTestFixture.HasLinkTo(listHtml, "/"));
        Assert.True(DinosaurPageTestFixture.HasLinkTo(detailHtml, "/"));
    }

    [Fact]
    public async Task Dinosaur_detail_renders_accessible_image_modal_contract()
    {
        string html = await _fixture.GetOkHtmlAsync("/dinosaurs/tyrannosaurus-rex");

        Assert.Contains("dino-image-button", html);
        Assert.Contains("aria-label=\"開啟暴龍大圖\"", html);
        Assert.Contains("data-dino-modal-open=\"dino-image-modal\"", html);
        Assert.Contains("id=\"dino-image-modal\"", html);
        Assert.Contains("modal fade", html);
        Assert.Contains("aria-label=\"關閉大圖\"", html);
        Assert.Contains("dino-modal__image", html);
        Assert.Contains("可愛繪本風暴龍站在綠色草地上張開微笑大嘴", html);
        Assert.Contains("dinosaurs", html);
    }

    [Fact]
    public async Task Dinosaur_list_renders_accessible_search_contract()
    {
        string html = await _fixture.GetOkHtmlAsync("/dinosaurs");

        Assert.Contains("aria-label=\"搜尋恐龍\"", html);
        Assert.Contains("data-dino-search-input", html);
        Assert.Contains("data-dino-search-item", html);
        Assert.Contains("data-dino-search-text", html);
        Assert.Contains("清除搜尋", html);
        Assert.Contains("data-dino-clear-search", html);
        Assert.Contains("沒有找到符合的史前朋友", html);
        Assert.Contains("史前飛行爬行類", html);
        Assert.True(DinosaurPageTestFixture.HasLinkTo(html, "/dinosaurs/pteranodon"));
    }
}
