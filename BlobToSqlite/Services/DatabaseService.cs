using Dapper;
using Microsoft.Data.Sqlite;

namespace BlobToSqlite.Services;

public class DatabaseService
{
    private readonly string _connectionString;

    public DatabaseService(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task InitializeAsync()
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        // For this iteration, we drop tables to ensure schema matches requirements.
        // In a real prod scenario, we would use migrations.
        await connection.ExecuteAsync("DROP TABLE IF EXISTS BlobData;");
        await connection.ExecuteAsync("DROP TABLE IF EXISTS Partners;");

        var sqlPartners = @"
            CREATE TABLE IF NOT EXISTS Partners (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL UNIQUE
            );";
        await connection.ExecuteAsync(sqlPartners);

        var sqlBlobData = @"
            CREATE TABLE IF NOT EXISTS BlobData (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                PartnerId INTEGER NOT NULL,
                EventType TEXT NOT NULL,
                EventDateTime DATETIME,
                FileName TEXT NOT NULL,
                JsonContent TEXT,
                ImportedAt TEXT NOT NULL,
                FOREIGN KEY(PartnerId) REFERENCES Partners(Id)
            );";

        await connection.ExecuteAsync(sqlBlobData);
    }

    public async Task<int> GetOrCreatePartnerIdAsync(string partnerName)
    {
        using var connection = new SqliteConnection(_connectionString);
        
        var sqlSelect = "SELECT Id FROM Partners WHERE Name = @Name;";
        var id = await connection.QuerySingleOrDefaultAsync<int?>(sqlSelect, new { Name = partnerName });

        if (id.HasValue)
        {
            return id.Value;
        }

        var sqlInsert = "INSERT INTO Partners (Name) VALUES (@Name); SELECT last_insert_rowid();";
        return await connection.QuerySingleAsync<int>(sqlInsert, new { Name = partnerName });
    }

    public async Task InsertBlobDataAsync(int partnerId, string eventType, DateTime? eventDateTime, string fileName, string jsonContent)
    {
        using var connection = new SqliteConnection(_connectionString);
        var sql = @"
            INSERT INTO BlobData (PartnerId, EventType, EventDateTime, FileName, JsonContent, ImportedAt)
            VALUES (@PartnerId, @EventType, @EventDateTime, @FileName, @JsonContent, @ImportedAt);";

        await connection.ExecuteAsync(sql, new
        {
            PartnerId = partnerId,
            EventType = eventType,
            EventDateTime = eventDateTime,
            FileName = fileName,
            JsonContent = jsonContent,
            ImportedAt = DateTime.UtcNow.ToString("o")
        });
    }
}
