using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace StoryBook.Tests.Integration;

public sealed class ExplorePageTestFixture : IAsyncLifetime
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
        string html = await response.Content.ReadAsStringAsync();
        return WebUtility.HtmlDecode(html);
    }

    public HttpClient CreateClientWithCatalogPaths(
        string dinosaurContentPath = "Data/dinosaurs.json",
        string aquariumContentPath = "Data/aquarium.json")
    {
        WebApplicationFactory<Program> factory = Factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((_, configurationBuilder) =>
            {
                configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
                {
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

    public static bool HasLinkTo(string html, string href)
    {
        return html.Contains($"href=\"{href}\"", StringComparison.OrdinalIgnoreCase)
            || html.Contains($"href='{href}'", StringComparison.OrdinalIgnoreCase);
    }

    public static int CountOccurrences(string value, string search)
    {
        int count = 0;
        int index = 0;

        while ((index = value.IndexOf(search, index, StringComparison.OrdinalIgnoreCase)) >= 0)
        {
            count++;
            index += search.Length;
        }

        return count;
    }
}
