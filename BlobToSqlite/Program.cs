using BlobToSqlite.Services;

using Microsoft.Extensions.Configuration;

namespace BlobToSqlite;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Starting Blob to SQLite Downloader...");

        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile("appsettings.local.json", optional: true, reloadOnChange: true);

        IConfiguration configuration = builder.Build();

        string blobConnectionString = configuration["AzureStorage:ConnectionString"];
        string containerName = configuration["AzureStorage:ContainerName"];
        string dbConnectionString = configuration["Database:ConnectionString"];

        if (string.IsNullOrEmpty(blobConnectionString) || string.IsNullOrEmpty(containerName))
        {
            Console.WriteLine("Error: Missing configuration. Please check appsettings.json.");
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

        // Initialize Services
        var blobStorage = new AzureBlobStorage(blobConnectionString);
        var dbService = new DatabaseService(dbConnectionString);
        var importService = new BlobImportService(blobStorage, dbService);

        // Initialize DB
        Console.WriteLine("Initializing Database...");
        await dbService.InitializeAsync();

        // Run Import
        int count = 0;
        try 
        {
            count = await importService.RunImportAsync(containerName, specificDate, minDate);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            Console.WriteLine("Please ensure AZURE_STORAGE_CONNECTION_STRING and AZURE_CONTAINER_NAME environment variables are set, or update the code.");
        }

        Console.WriteLine($"Finished. Processed {count} files.");
    }
}
