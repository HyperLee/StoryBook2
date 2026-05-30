namespace StoryBook.Tests.Integration;

public sealed class QuizPagesTests : IClassFixture<QuizPageTestFixture>
{
    private readonly QuizPageTestFixture _fixture;

    public QuizPagesTests(QuizPageTestFixture fixture)
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

    [Fact]
    public async Task Quiz_page_renders_friendly_empty_state_for_empty_catalog()
    {
        using TempQuizCatalog catalog = QuizPageTestFixture.CreateTempQuizCatalog(
            """
            { "version": 1, "questions": [] }
            """);
        using HttpClient client = _fixture.CreateClientWithCatalogPaths(catalog.Path);

        string html = await GetOkHtmlAsync(client, "/quiz");

        Assert.Contains("data-quiz-empty", html);
        Assert.Contains("問答題目正在準備中", html);
        Assert.DoesNotContain("System.", html);
        Assert.DoesNotContain(catalog.Path, html);
    }

    [Fact]
    public async Task Quiz_page_renders_friendly_state_for_invalid_question_file_without_internal_details()
    {
        using TempQuizCatalog catalog = QuizPageTestFixture.CreateTempQuizCatalog("{ not valid json");
        using HttpClient client = _fixture.CreateClientWithCatalogPaths(catalog.Path);

        string html = await GetOkHtmlAsync(client, "/quiz");

        Assert.Contains("data-quiz-empty", html);
        Assert.DoesNotContain("JsonException", html, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("System.Text.Json", html, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(catalog.Path, html);
    }

    [Fact]
    public async Task Quiz_page_filters_unavailable_story_references_and_unknown_question_id_falls_back()
    {
        using TempQuizCatalog catalog = QuizPageTestFixture.CreateTempQuizCatalog(
            """
            {
              "version": 1,
              "questions": [
                {
                  "id": "missing-story-question",
                  "source": "dinosaurs",
                  "difficulty": "easy",
                  "sortOrder": 1,
                  "prompt": { "zhTW": "題目", "en": "Question" },
                  "options": [
                    { "id": "one", "text": { "zhTW": "一", "en": "One" }, "sortOrder": 1 },
                    { "id": "two", "text": { "zhTW": "二", "en": "Two" }, "sortOrder": 2 }
                  ],
                  "correctOptionId": "one",
                  "correctFeedback": { "zhTW": "答對了", "en": "Correct" },
                  "incorrectFeedback": { "zhTW": "再想想", "en": "Try again" },
                  "explanation": { "zhTW": "解釋", "en": "Explanation" },
                  "relatedStories": [
                    { "source": "dinosaurs", "slug": "missing-story", "sortOrder": 1 }
                  ]
                }
              ]
            }
            """);
        using HttpClient client = _fixture.CreateClientWithCatalogPaths(catalog.Path);

        string unavailableHtml = await GetOkHtmlAsync(client, "/quiz");
        string unknownQuestionHtml = await _fixture.GetOkHtmlAsync("/quiz?scope=dinosaurs&questionId=not-here");

        Assert.Contains("data-quiz-empty", unavailableHtml);
        Assert.DoesNotContain("/missing-story", unavailableHtml, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("data-quiz-question-fallback", unknownQuestionHtml);
        Assert.Contains("data-quiz-current-question-id=\"tyrannosaurus-teeth\"", unknownQuestionHtml);
    }

    private static bool HasLinkTo(string html, string href)
    {
        return html.Contains($"href=\"{href}\"", StringComparison.OrdinalIgnoreCase)
            || html.Contains($"href='{href}'", StringComparison.OrdinalIgnoreCase);
    }

    private static async Task<string> GetOkHtmlAsync(HttpClient client, string path)
    {
        using HttpResponseMessage response = await client.GetAsync(path);

        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        return System.Net.WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());
    }
}
