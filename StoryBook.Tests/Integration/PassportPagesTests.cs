namespace StoryBook.Tests.Integration;

public sealed class PassportPagesTests : IClassFixture<PassportPageTestFixture>
{
    private readonly PassportPageTestFixture _fixture;

    public PassportPagesTests(PassportPageTestFixture fixture)
    {
        _fixture = fixture;
    }
}
