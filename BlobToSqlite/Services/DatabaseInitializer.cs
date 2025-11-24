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
    private readonly IDbConnectionFactory _connectionFactory;

    public DatabaseInitializer(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task InitializeAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        // OpenAsync is not part of IDbConnection, but most implementations have it. 
        // However, Dapper works with IDbConnection which might not be open.
        // We cast to DbConnection to use OpenAsync if possible, or just Open.
        if (connection is System.Data.Common.DbConnection dbConnection)
        {
            await dbConnection.OpenAsync();
        }
        else
        {
            connection.Open();
        }

        // For this iteration, we drop tables to ensure schema matches requirements.
        await connection.ExecuteAsync("DROP TABLE IF EXISTS BlobData;");
        await connection.ExecuteAsync("DROP TABLE IF EXISTS Partners;");

        var isSqlite = _connectionFactory.GetProviderName().Equals("Sqlite", StringComparison.OrdinalIgnoreCase);

        var identityType = isSqlite ? "INTEGER PRIMARY KEY AUTOINCREMENT" : "INT IDENTITY(1,1) PRIMARY KEY";
        var textType = isSqlite ? "TEXT" : "NVARCHAR(MAX)";
        var dateTimeType = isSqlite ? "DATETIME" : "DATETIME2";

        var sqlPartners = $@"
            CREATE TABLE Partners (
                Id {identityType},
                Name {textType} NOT NULL UNIQUE
            );";
        
        // SQL Server doesn't support "CREATE TABLE IF NOT EXISTS" directly in older versions or standard SQL.
        // But since we drop tables above, we can just CREATE.
        // For robustness, we could check existence, but dropping is fine for this tool's scope as per previous logic.
        
        await connection.ExecuteAsync(sqlPartners);

        var sqlBlobData = $@"
            CREATE TABLE BlobData (
                Id {identityType},
                PartnerId {(isSqlite ? "INTEGER" : "INT")} NOT NULL,
                EventType {textType} NOT NULL,
                EventDateTime {dateTimeType},
                FileName {textType} NOT NULL,
                JsonContent {textType},
                ImportedAt {textType} NOT NULL,
                FOREIGN KEY(PartnerId) REFERENCES Partners(Id)
            );";

        await connection.ExecuteAsync(sqlBlobData);
    }
}
