namespace StoryBook.Tests.Integration;

public sealed class ThemePagesTests : IClassFixture<DinosaurPageTestFixture>
{
    private readonly DinosaurPageTestFixture _fixture;

    public ThemePagesTests(DinosaurPageTestFixture fixture)
    {
        _fixture = fixture;
    }
}
