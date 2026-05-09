using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StoryBook.Models;
using StoryBook.Services;
using StoryBook.Tests.Support;

namespace StoryBook.Tests.Unit;

public sealed class DinosaurCatalogServiceTests
{
    [Fact]
    public void GetProfiles_loads_json_once_and_returns_sort_ordered_profiles()
    {
        DinosaurCatalogService service = CreateService();

        IReadOnlyList<DinosaurProfile> profiles = service.GetProfiles();
        IReadOnlyList<DinosaurProfile> secondRead = service.GetProfiles();

        Assert.Equal(8, profiles.Count);
        Assert.Same(profiles, secondRead);
        Assert.Equal(profiles.OrderBy(profile => profile.SortOrder).Select(profile => profile.Slug), profiles.Select(profile => profile.Slug));
    }

    [Fact]
    public void TryGetBySlug_resolves_known_slug_and_logs_unknown_slug()
    {
        RecordingLogger<DinosaurCatalogService> logger = new();
        DinosaurCatalogService service = CreateService(logger);

        bool foundKnown = service.TryGetBySlug("tyrannosaurus-rex", out DinosaurProfile? known);
        bool foundUnknown = service.TryGetBySlug("not-a-real-slug", out DinosaurProfile? unknown);

        Assert.True(foundKnown);
        Assert.Equal("暴龍", known?.Names.Get(LanguageCode.ZhTW));
        Assert.False(foundUnknown);
        Assert.Null(unknown);
        Assert.True(logger.Contains(LogLevel.Warning, "not-a-real-slug"));
    }

    [Fact]
    public void Localized_text_falls_back_to_chinese_when_requested_field_is_missing()
    {
        DinosaurText text = new()
        {
            ZhTW = "中文內容",
            En = ""
        };

        Assert.Equal("中文內容", text.Get(LanguageCode.En));
        Assert.Equal("中文內容", text.Get("bad-value"));
    }

    [Fact]
    public void Cached_slug_lookup_and_search_have_p95_under_200ms()
    {
        DinosaurCatalogService service = CreateService();
        _ = service.GetProfiles();
        List<long> durations = [];

        for (int i = 0; i < 100; i++)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            Assert.True(service.TryGetBySlug("pteranodon", out _));
            Assert.NotEmpty(service.Search("翼龍", LanguageCode.ZhTW));
            stopwatch.Stop();
            durations.Add(stopwatch.ElapsedMilliseconds);
        }

        long p95 = durations.Order().ElementAt(94);
        Assert.True(p95 < 200, $"p95 was {p95}ms");
    }

    [Fact]
    public void GetFirstProfile_returns_lowest_sort_order_and_summaries_stay_readable()
    {
        DinosaurCatalogService service = CreateService();

        DinosaurProfile first = service.GetFirstProfile();

        Assert.Equal("tyrannosaurus-rex", first.Slug);
        Assert.Equal(1, first.SortOrder);
        Assert.InRange(DinosaurContentValidator.CountReadableUnits(first.Summary.ZhTW, LanguageCode.ZhTW), 1, 200);
        Assert.InRange(DinosaurContentValidator.CountReadableUnits(first.Summary.En, LanguageCode.En), 1, 200);
    }

    [Fact]
    public void Previous_and_next_navigation_handles_first_middle_last_and_unknown_slugs()
    {
        DinosaurCatalogService service = CreateService();

        Assert.Null(service.GetPreviousProfile("tyrannosaurus-rex"));
        Assert.Equal("triceratops", service.GetNextProfile("tyrannosaurus-rex")?.Slug);

        Assert.Equal("tyrannosaurus-rex", service.GetPreviousProfile("triceratops")?.Slug);
        Assert.Equal("stegosaurus", service.GetNextProfile("triceratops")?.Slug);

        Assert.Equal("ankylosaurus", service.GetPreviousProfile("parasaurolophus")?.Slug);
        Assert.Null(service.GetNextProfile("parasaurolophus"));

        Assert.Null(service.GetPreviousProfile("not-a-real-slug"));
        Assert.Null(service.GetNextProfile("not-a-real-slug"));
    }

    private static DinosaurCatalogService CreateService(RecordingLogger<DinosaurCatalogService>? logger = null)
    {
        return new DinosaurCatalogService(
            Options.Create(new DinosaurCatalogOptions()),
            new FakeWebHostEnvironment(TestPaths.StoryBookRoot),
            new DinosaurContentValidator(),
            logger ?? new RecordingLogger<DinosaurCatalogService>());
    }
}
