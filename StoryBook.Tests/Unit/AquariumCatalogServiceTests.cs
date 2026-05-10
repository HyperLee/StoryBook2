using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StoryBook.Models;
using StoryBook.Services;
using StoryBook.Tests.Support;

namespace StoryBook.Tests.Unit;

public sealed class AquariumCatalogServiceTests
{
    [Fact]
    public void GetProfiles_loads_json_once_and_returns_sort_ordered_profiles()
    {
        AquariumCatalogService service = CreateService();

        IReadOnlyList<AquariumAnimalProfile> profiles = service.GetProfiles();
        IReadOnlyList<AquariumAnimalProfile> secondRead = service.GetProfiles();

        Assert.Equal(15, profiles.Count);
        Assert.Same(profiles, secondRead);
        Assert.Equal(profiles.OrderBy(profile => profile.SortOrder).Select(profile => profile.Slug), profiles.Select(profile => profile.Slug));
    }

    [Fact]
    public void Load_failure_logs_path_and_throws_clear_exception()
    {
        RecordingLogger<AquariumCatalogService> logger = new();
        AquariumCatalogService service = CreateService(logger, "Data/missing-aquarium.json");

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => service.GetProfiles());

        Assert.Contains("Aquarium content catalog could not be loaded", exception.Message);
        Assert.True(logger.Contains(LogLevel.Error, "missing-aquarium.json"));
    }

    [Fact]
    public void TryGetBySlug_resolves_known_slug_and_logs_unknown_slug()
    {
        RecordingLogger<AquariumCatalogService> logger = new();
        AquariumCatalogService service = CreateService(logger);

        bool foundKnown = service.TryGetBySlug("clownfish", out AquariumAnimalProfile? known);
        bool foundUnknown = service.TryGetBySlug("not-a-real-slug", out AquariumAnimalProfile? unknown);

        Assert.True(foundKnown);
        Assert.Equal("小丑魚", known?.Names.Get(LanguageCode.ZhTW));
        Assert.False(foundUnknown);
        Assert.Null(unknown);
        Assert.True(logger.Contains(LogLevel.Warning, "not-a-real-slug"));
    }

    [Fact]
    public void Previous_and_next_navigation_handles_first_middle_last_and_unknown_slugs()
    {
        AquariumCatalogService service = CreateService();

        Assert.Equal("clownfish", service.GetFirstProfile().Slug);
        Assert.Null(service.GetPreviousProfile("clownfish"));
        Assert.Equal("seahorse", service.GetNextProfile("clownfish")?.Slug);

        Assert.Equal("clownfish", service.GetPreviousProfile("seahorse")?.Slug);
        Assert.Equal("sea-turtle", service.GetNextProfile("seahorse")?.Slug);

        Assert.Equal("goldfish", service.GetPreviousProfile("axolotl")?.Slug);
        Assert.Null(service.GetNextProfile("axolotl"));

        Assert.Null(service.GetPreviousProfile("not-a-real-slug"));
        Assert.Null(service.GetNextProfile("not-a-real-slug"));
    }

    [Fact]
    public void Repeated_previous_next_navigation_resolves_final_valid_profile()
    {
        AquariumCatalogService service = CreateService();
        AquariumAnimalProfile current = service.GetFirstProfile();

        for (int i = 0; i < 4; i++)
        {
            current = Assert.IsType<AquariumAnimalProfile>(service.GetNextProfile(current.Slug));
        }

        Assert.Equal("octopus", current.Slug);

        for (int i = 0; i < 2; i++)
        {
            current = Assert.IsType<AquariumAnimalProfile>(service.GetPreviousProfile(current.Slug));
        }

        Assert.Equal("sea-turtle", current.Slug);
    }

    [Fact]
    public void Search_matches_localized_fields_keywords_whitespace_punctuation_and_short_queries()
    {
        AquariumCatalogService service = CreateService();

        Assert.Equal("seahorse", Assert.Single(service.Search(" 海，馬! ", LanguageCode.ZhTW)).Slug);
        Assert.Equal("seahorse", Assert.Single(service.Search(" 海 馬 ", LanguageCode.ZhTW)).Slug);
        Assert.Contains(service.Search("ReEf", LanguageCode.En), profile => profile.Slug == "clownfish");
        Assert.Contains(service.Search("REEF!!!", LanguageCode.En), profile => profile.Slug == "coral");
        Assert.Contains(service.Search("plankton", LanguageCode.En), profile => profile.Slug == "jellyfish");
        Assert.Equal("axolotl", Assert.Single(service.Search("六角恐龍", LanguageCode.ZhTW)).Slug);
        Assert.Empty(service.Search("沒有這種動物", LanguageCode.ZhTW));
        Assert.Equal(15, service.Search("   ", LanguageCode.En).Count);
        Assert.Empty(service.Search("a", LanguageCode.En));
        Assert.Empty(service.Search("海", LanguageCode.ZhTW));
        Assert.Empty(service.Search("！", LanguageCode.ZhTW));
        Assert.True(service.IsQueryTooShort("a"));
        Assert.True(service.IsQueryTooShort("！"));
        Assert.False(service.IsQueryTooShort("海馬"));
    }

    [Fact]
    public void Rapid_search_queries_use_last_query_results()
    {
        AquariumCatalogService service = CreateService();
        string[] queries = ["re", "reef", "axolotl"];
        IReadOnlyList<AquariumAnimalProfile> results = [];

        foreach (string query in queries)
        {
            results = service.Search(query, LanguageCode.En);
        }

        AquariumAnimalProfile result = Assert.Single(results);
        Assert.Equal("axolotl", result.Slug);
    }

    [Fact]
    public void GetSearchResults_projects_bilingual_client_search_text_and_category_names()
    {
        AquariumCatalogService service = CreateService();

        AquariumSearchResult result = Assert.Single(service.GetSearchResults(LanguageCode.ZhTW), item => item.Slug == "clownfish");

        Assert.Equal("小丑魚", result.NameZhTW);
        Assert.Equal("Clownfish", result.NameEn);
        Assert.Equal("珊瑚礁", result.HabitatCategoryNameZhTW);
        Assert.Equal("Coral reef", result.HabitatCategoryNameEn);
        Assert.Contains("coral", result.SearchText, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Cached_slug_lookup_and_search_have_p95_under_200ms()
    {
        AquariumCatalogService service = CreateService();
        _ = service.GetProfiles();
        List<long> durations = [];

        for (int i = 0; i < 100; i++)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            Assert.True(service.TryGetBySlug("clownfish", out _));
            Assert.NotEmpty(service.Search("reef", LanguageCode.En));
            stopwatch.Stop();
            durations.Add(stopwatch.ElapsedMilliseconds);
        }

        long p95 = durations.Order().ElementAt(94);
        Assert.True(p95 < 200, $"p95 was {p95}ms");
    }

    private static AquariumCatalogService CreateService(
        RecordingLogger<AquariumCatalogService>? logger = null,
        string contentPath = "Data/aquarium.json")
    {
        return new AquariumCatalogService(
            Options.Create(new AquariumCatalogOptions { ContentPath = contentPath }),
            new FakeWebHostEnvironment(TestPaths.StoryBookRoot),
            new AquariumContentValidator(),
            logger ?? new RecordingLogger<AquariumCatalogService>());
    }
}
