using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Testing;

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

    [Fact]
    public async Task Quiz_answer_post_with_antiforgery_renders_correct_feedback_explanation_and_next_link()
    {
        string html = await _fixture.PostAnswerAsync(
            "/quiz?scope=dinosaurs&questionId=tyrannosaurus-teeth",
            "dinosaurs",
            "tyrannosaurus-teeth",
            "sharp-teeth");

        Assert.Contains("data-quiz-feedback", html);
        Assert.Contains("data-quiz-answer-state=\"correct\"", html);
        Assert.Contains("答對了", html);
        Assert.Contains("暴龍是肉食性恐龍", html);
        Assert.Contains("data-quiz-next-question", html);
        Assert.DoesNotContain("score", html, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("progress", html, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Quiz_answer_post_renders_wrong_feedback_and_no_selection_prompt()
    {
        string wrongHtml = await _fixture.PostAnswerAsync(
            "/quiz?scope=dinosaurs&questionId=tyrannosaurus-teeth",
            "dinosaurs",
            "tyrannosaurus-teeth",
            "flat-shell");
        string noSelectionHtml = await _fixture.PostAnswerAsync(
            "/quiz?scope=dinosaurs&questionId=tyrannosaurus-teeth",
            "dinosaurs",
            "tyrannosaurus-teeth",
            selectedOptionId: null);

        Assert.Contains("data-quiz-answer-state=\"incorrect\"", wrongHtml);
        Assert.Contains("再想想看", wrongHtml);
        Assert.Contains("暴龍是肉食性恐龍", wrongHtml);
        Assert.Contains("data-quiz-answer-state=\"needs-selection\"", noSelectionHtml);
        Assert.Contains("請先選一個答案", noSelectionHtml);
        Assert.DoesNotContain("data-quiz-answer-state=\"incorrect\"", noSelectionHtml);
    }

    [Fact]
    public async Task Quiz_page_renders_related_story_anchor_metadata_and_source_labels()
    {
        string dinosaurHtml = await _fixture.GetOkHtmlAsync("/quiz?scope=dinosaurs&questionId=triceratops-horns");
        string aquariumHtml = await _fixture.GetOkHtmlAsync("/quiz?scope=aquarium&questionId=clownfish-home");

        Assert.Contains("data-quiz-related-stories", dinosaurHtml);
        Assert.Contains("data-quiz-related-story", dinosaurHtml);
        Assert.Contains("data-quiz-related-source=\"dinosaurs\"", dinosaurHtml);
        Assert.Contains("data-quiz-related-slug=\"triceratops\"", dinosaurHtml);
        Assert.True(HasLinkTo(dinosaurHtml, "/dinosaurs/triceratops"));
        Assert.Contains("去讀三角龍故事", dinosaurHtml);
        Assert.Contains("恐龍", dinosaurHtml);

        Assert.Contains("data-quiz-related-source=\"aquarium\"", aquariumHtml);
        Assert.Contains("data-quiz-related-slug=\"clownfish\"", aquariumHtml);
        Assert.True(HasLinkTo(aquariumHtml, "/aquarium/clownfish"));
        Assert.Contains("去讀小丑魚故事", aquariumHtml);
        Assert.Contains("水族館", aquariumHtml);
    }

    [Fact]
    public async Task Quiz_related_story_anchors_do_not_render_blank_or_placeholder_hrefs()
    {
        string html = await _fixture.GetOkHtmlAsync("/quiz");

        Assert.DoesNotContain("data-quiz-related-story href=\"\"", html, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("data-quiz-related-story href=\"#\"", html, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("/missing-story", html, StringComparison.OrdinalIgnoreCase);
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

        public async Task<string> PostAnswerAsync(
            string getPath,
            string scope,
            string questionId,
            string? selectedOptionId)
        {
            string formHtml = await GetOkHtmlAsync(getPath);
            string token = ExtractHiddenValue(formHtml, "__RequestVerificationToken");
            List<KeyValuePair<string, string>> fields =
            [
                new("__RequestVerificationToken", token),
                new("scope", scope),
                new("questionId", questionId)
            ];

            if (!string.IsNullOrWhiteSpace(selectedOptionId))
            {
                fields.Add(new KeyValuePair<string, string>("selectedOptionId", selectedOptionId));
            }

            using FormUrlEncodedContent content = new(fields);
            using HttpResponseMessage response = await Client.PostAsync("/quiz?handler=Answer", content);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            return WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());
        }
    }

    private static bool HasLinkTo(string html, string href)
    {
        return html.Contains($"href=\"{href}\"", StringComparison.OrdinalIgnoreCase)
            || html.Contains($"href='{href}'", StringComparison.OrdinalIgnoreCase);
    }

    private static string ExtractHiddenValue(string html, string name)
    {
        Match match = Regex.Match(
            html,
            $"<input[^>]*name=\"{Regex.Escape(name)}\"[^>]*value=\"(?<value>[^\"]+)\"",
            RegexOptions.IgnoreCase);

        Assert.True(match.Success, $"Expected hidden input named {name}.");
        return match.Groups["value"].Value;
    }
}
