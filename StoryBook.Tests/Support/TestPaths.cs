namespace StoryBook.Tests.Support;

internal static class TestPaths
{
    public static string RepositoryRoot
    {
        get
        {
            string directory = AppContext.BaseDirectory;

            while (!File.Exists(Path.Combine(directory, "StoryBook2.sln")))
            {
                DirectoryInfo? parent = Directory.GetParent(directory);

                if (parent is null)
                {
                    throw new DirectoryNotFoundException("Could not locate StoryBook2.sln from test output directory.");
                }

                directory = parent.FullName;
            }

            return directory;
        }
    }

    public static string StoryBookRoot => Path.Combine(RepositoryRoot, "StoryBook");
}
