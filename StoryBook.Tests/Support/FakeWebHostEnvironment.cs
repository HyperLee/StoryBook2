using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;

namespace StoryBook.Tests.Support;

internal sealed class FakeWebHostEnvironment : IWebHostEnvironment
{
    public FakeWebHostEnvironment(string contentRootPath)
    {
        ContentRootPath = contentRootPath;
        WebRootPath = Path.Combine(contentRootPath, "wwwroot");
        ContentRootFileProvider = new PhysicalFileProvider(contentRootPath);
        WebRootFileProvider = Directory.Exists(WebRootPath)
            ? new PhysicalFileProvider(WebRootPath)
            : new NullFileProvider();
    }

    public string ApplicationName { get; set; } = "StoryBook";

    public IFileProvider ContentRootFileProvider { get; set; }

    public string ContentRootPath { get; set; }

    public string EnvironmentName { get; set; } = "Development";

    public IFileProvider WebRootFileProvider { get; set; }

    public string WebRootPath { get; set; }
}
