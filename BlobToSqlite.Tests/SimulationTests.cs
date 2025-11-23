using BlobToSqlite.Configuration;
using BlobToSqlite.Services;
using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;

namespace BlobToSqlite.Tests;

public class SimulationTests
{
    [Fact]
    public async Task RunSimulation_ShouldImportFiles_FromInputToOutputDb()
    {
        // Arrange
        // Navigate up from bin/Debug/net8.0 to the solution root, then to BlobToSqlite project
        var testProjectDir = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName;
        var solutionDir = Directory.GetParent(testProjectDir).FullName;
        var projectDir = Path.Combine(solutionDir, "BlobToSqlite");
        
        var inputDir = Path.Combine(projectDir, "Input");
        var outputDir = Path.Combine(projectDir, "Output");
        var dbPath = Path.Combine(outputDir, "blobs.db");
        var connectionString = $"Data Source={dbPath}";

        // Ensure directories exist
        Directory.CreateDirectory(inputDir);
        Directory.CreateDirectory(outputDir);

        // Create Dummy Data with CORRECT structure: Partner/Event/Date/File.json
        // We use a specific test partner to avoid conflict with user data
        var partnerDir = Path.Combine(inputDir, "TestPartner", "TestEvent", "2025-11-23");
        Directory.CreateDirectory(partnerDir);
        var jsonContent = "{\"key\": \"value\", \"test\": \"true\"}";
        var testFilePath = Path.Combine(partnerDir, "test_file.json");
        await File.WriteAllTextAsync(testFilePath, jsonContent);

        // Setup Services
        var blobStorage = new LocalFileBlobStorage(inputDir);
        
        // Configuration for Repository
        var dbSettings = Options.Create(new DatabaseSettings { ConnectionString = connectionString });
        var blobRepository = new BlobRepository(dbSettings);
        var dbInitializer = new DatabaseInitializer(dbSettings);
        var pathParser = new StandardBlobPathParser();

        var importService = new BlobImportService(blobStorage, blobRepository, pathParser);

        // Act
        await dbInitializer.InitializeAsync();
        int count = await importService.RunImportAsync("dummy-container", null, null);

        // Assert
        // We expect at least 1 file (our test file). There might be others if the user added them.
        Assert.True(count >= 1, $"Expected at least 1 file imported, but got {count}");

        using var connection = new SqliteConnection(connectionString);
        var record = await connection.QueryFirstOrDefaultAsync("SELECT * FROM BlobData WHERE FileName = 'test_file.json'");
        
        Assert.NotNull(record);
        Assert.Equal("TestEvent", record.EventType);
        Assert.Equal(jsonContent, record.JsonContent);
        
        // Cleanup (Optional: remove test file to keep Input clean, or leave it for inspection)
        // File.Delete(testFilePath);
        // Directory.Delete(partnerDir, true); 
    }
}
