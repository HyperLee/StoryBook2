using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace StoryBook.Tests.Integration;

public sealed class QuizPageTestFixture : IAsyncLifetime
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

    public HttpClient CreateClientWithCatalogPaths(
        string quizContentPath,
        string dinosaurContentPath = "Data/dinosaurs.json",
        string aquariumContentPath = "Data/aquarium.json")
    {
        WebApplicationFactory<Program> factory = Factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((_, configurationBuilder) =>
            {
                configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["QuizCatalog:ContentPath"] = quizContentPath,
                    ["DinosaurCatalog:ContentPath"] = dinosaurContentPath,
                    ["AquariumCatalog:ContentPath"] = aquariumContentPath
                });
            });
        });

        return factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    public static TempQuizCatalog CreateTempQuizCatalog(string json)
    {
        string path = Path.Combine(Path.GetTempPath(), $"storybook-quiz-page-{Guid.NewGuid():N}.json");
        File.WriteAllText(path, json);
        return new TempQuizCatalog(path);
    }

    public static string ExtractHiddenValue(string html, string name)
    {
        Match match = Regex.Match(
            html,
            $"<input[^>]*name=\"{Regex.Escape(name)}\"[^>]*value=\"(?<value>[^\"]+)\"",
            RegexOptions.IgnoreCase);

        Assert.True(match.Success, $"Expected hidden input named {name}.");
        return match.Groups["value"].Value;
    }
}

public sealed class TempQuizCatalog : IDisposable
{
    public TempQuizCatalog(string path)
    {
        Path = path;
    }

    public string Path { get; }

    public void Dispose()
    {
        if (File.Exists(Path))
        {
            File.Delete(Path);
        }
    }
}
