using BlobToSqlite.Configuration;
using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;

namespace BlobToSqlite.Services;

public interface IDatabaseInitializer
{
    Task InitializeAsync();
}

public class DatabaseInitializer : IDatabaseInitializer
{
    private readonly string _connectionString;

    public DatabaseInitializer(IOptions<DatabaseSettings> settings)
    {
        _connectionString = settings.Value.ConnectionString;
    }

    public async Task InitializeAsync()
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        // For this iteration, we drop tables to ensure schema matches requirements.
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
}
