using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using StoryBook.Tests.Support;

namespace StoryBook.Tests.Integration;

public sealed class QuizPagesTests : IClassFixture<QuizPagesTests.QuizPageFixture>
{
    private readonly QuizPageFixture _fixture;

    public QuizPagesTests(QuizPageFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Home_and_explore_pages_expose_normal_quiz_entry_links()
    {
        string homeHtml = await _fixture.GetOkHtmlAsync("/");
        string exploreHtml = await _fixture.GetOkHtmlAsync("/explore");

        Assert.Contains("問答挑戰", homeHtml);
        Assert.Contains("Quiz challenge", homeHtml);
        Assert.True(HasLinkTo(homeHtml, "/quiz"));
        Assert.Contains("問答挑戰", exploreHtml);
        Assert.True(HasLinkTo(exploreHtml, "/quiz"));
    }

    [Fact]
    public async Task Quiz_page_renders_initial_question_scope_links_and_dom_metadata()
    {
        string html = await _fixture.GetOkHtmlAsync("/quiz");

        Assert.Contains("data-quiz-page", html);
        Assert.Contains("data-quiz-scope=\"all\"", html);
        Assert.Contains("data-quiz-current-question-id=", html);
        Assert.Contains("data-quiz-scope-nav", html);
        Assert.True(HasLinkTo(html, "/quiz?scope=all"));
        Assert.True(HasLinkTo(html, "/quiz?scope=dinosaurs"));
        Assert.True(HasLinkTo(html, "/quiz?scope=aquarium"));
        Assert.Contains("data-quiz-question", html);
        Assert.Contains("data-quiz-source=\"", html);
        Assert.Contains("data-quiz-difficulty=\"easy\"", html);
        Assert.Contains("name=\"selectedOptionId\"", html);
        Assert.Contains("data-quiz-next-question", html);
        Assert.DoesNotContain("correctOptionId", html, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("data-correct", html, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Quiz_page_filters_dinosaur_scope_and_invalid_scope_falls_back_to_all()
    {
        string dinosaurHtml = await _fixture.GetOkHtmlAsync("/quiz?scope=dinosaurs");
        string invalidHtml = await _fixture.GetOkHtmlAsync("/quiz?scope=space");

        Assert.Contains("data-quiz-scope=\"dinosaurs\"", dinosaurHtml);
        Assert.Contains("data-quiz-source=\"dinosaurs\"", dinosaurHtml);
        Assert.Contains("data-quiz-scope=\"all\"", invalidHtml);
        Assert.Contains("data-quiz-scope-fallback", invalidHtml);
    }

    public sealed class QuizPageFixture : IAsyncLifetime
    {
        public WebApplicationFactory<Program> Factory { get; private set; } = null!;

        public HttpClient Client { get; private set; } = null!;

        public Task InitializeAsync()
        {
            Factory = new WebApplicationFactory<Program>();
            Client = Factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });

            return Task.CompletedTask;
        }

        public Task DisposeAsync()
        {
            Client.Dispose();
            Factory.Dispose();
            return Task.CompletedTask;
        }

        public async Task<string> GetOkHtmlAsync(string path)
        {
            using HttpResponseMessage response = await Client.GetAsync(path);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            return WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());
        }
    }

    private static bool HasLinkTo(string html, string href)
    {
        return html.Contains($"href=\"{href}\"", StringComparison.OrdinalIgnoreCase)
            || html.Contains($"href='{href}'", StringComparison.OrdinalIgnoreCase);
    }
}
