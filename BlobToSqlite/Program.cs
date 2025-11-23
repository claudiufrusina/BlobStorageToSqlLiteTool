using BlobToSqlite.Configuration;
using BlobToSqlite.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace BlobToSqlite;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Starting Blob to SQLite Downloader...");

        var builder = Host.CreateApplicationBuilder(args);

        // Configuration
        builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                             .AddJsonFile("appsettings.local.json", optional: true, reloadOnChange: true);

        builder.Services.Configure<AzureStorageSettings>(builder.Configuration.GetSection("AzureStorage"));
        builder.Services.Configure<DatabaseSettings>(builder.Configuration.GetSection("Database"));

        // Services
        builder.Services.AddSingleton<IBlobStorage, AzureBlobStorage>();
        builder.Services.AddSingleton<IBlobPathParser, StandardBlobPathParser>();
        builder.Services.AddSingleton<IDatabaseInitializer, DatabaseInitializer>();
        builder.Services.AddSingleton<IBlobRepository, BlobRepository>();
        builder.Services.AddSingleton<BlobImportService>();

        using IHost host = builder.Build();

        // Initialize DB
        Console.WriteLine("Initializing Database...");
        var dbInitializer = host.Services.GetRequiredService<IDatabaseInitializer>();
        await dbInitializer.InitializeAsync();

        // Resolve Settings for manual check (optional, but good for user feedback)
        var azureSettings = host.Services.GetRequiredService<IOptions<AzureStorageSettings>>().Value;
        if (string.IsNullOrEmpty(azureSettings.ConnectionString) || string.IsNullOrEmpty(azureSettings.ContainerName))
        {
             Console.WriteLine("Error: Missing Azure configuration. Please check appsettings.json.");
             return;
        }

        // Date Filtering Logic
        DateTime? specificDate = null;
        DateTime minDate = DateTime.MinValue;

        if (args.Length > 0 && DateTime.TryParse(args[0], out DateTime parsedDate))
        {
            specificDate = parsedDate.Date;
            Console.WriteLine($"Filter: Processing only blobs for date {specificDate:yyyy-MM-dd}");
        }
        else
        {
            minDate = DateTime.UtcNow.Date.AddDays(-30);
            Console.WriteLine($"Filter: Processing blobs from the last 30 days (since {minDate:yyyy-MM-dd})");
        }

        // Run Import
        var importService = host.Services.GetRequiredService<BlobImportService>();
        int count = 0;
        try 
        {
            count = await importService.RunImportAsync(azureSettings.ContainerName, specificDate, minDate);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }

        Console.WriteLine($"Finished. Processed {count} files.");
    }
}
