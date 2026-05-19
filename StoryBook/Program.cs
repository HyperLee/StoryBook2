using StoryBook.Services;

namespace StoryBook;

public partial class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddRazorPages();
        builder.Services.Configure<DinosaurCatalogOptions>(
            builder.Configuration.GetSection(DinosaurCatalogOptions.SectionName));
        builder.Services.Configure<AquariumCatalogOptions>(
            builder.Configuration.GetSection(AquariumCatalogOptions.SectionName));
        builder.Services.AddSingleton<DinosaurContentValidator>();
        builder.Services.AddSingleton<DinosaurCatalogService>();
        builder.Services.AddSingleton<AquariumContentValidator>();
        builder.Services.AddSingleton<AquariumCatalogService>();
        builder.Services.AddSingleton<ExplorationCatalogService>();
        builder.Services.AddSingleton<ComparisonCatalogService>();
        builder.Services.AddSingleton<IComparisonCatalogService>(provider =>
            provider.GetRequiredService<ComparisonCatalogService>());
        builder.Services.AddSingleton<ExplorationSearchService>();
        builder.Services.AddSingleton<LanguagePreferenceService>();
        builder.Services.AddSingleton<ThemePreferenceService>();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();

        app.UseRouting();

        app.UseAuthorization();

        app.MapStaticAssets();
        app.MapRazorPages()
           .WithStaticAssets();

        app.Run();
    }
}
