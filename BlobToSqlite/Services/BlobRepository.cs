using BlobToSqlite.Configuration;
using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;

namespace BlobToSqlite.Services;

public interface IBlobRepository
{
    Task<int> GetOrCreatePartnerIdAsync(string partnerName);
    Task InsertBlobDataAsync(int partnerId, string eventType, DateTime? eventDateTime, string fileName, string jsonContent);
}

public class BlobRepository : IBlobRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public BlobRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<int> GetOrCreatePartnerIdAsync(string partnerName)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        var sqlSelect = "SELECT Id FROM Partners WHERE Name = @Name;";
        var id = await connection.QuerySingleOrDefaultAsync<int?>(sqlSelect, new { Name = partnerName });

        if (id.HasValue)
        {
            return id.Value;
        }

        var isSqlite = _connectionFactory.GetProviderName().Equals("Sqlite", StringComparison.OrdinalIgnoreCase);
        var identityQuery = isSqlite ? "last_insert_rowid()" : "CAST(SCOPE_IDENTITY() as int)";

        var sqlInsert = $"INSERT INTO Partners (Name) VALUES (@Name); SELECT {identityQuery};";
        return await connection.QuerySingleAsync<int>(sqlInsert, new { Name = partnerName });
    }

    public async Task InsertBlobDataAsync(int partnerId, string eventType, DateTime? eventDateTime, string fileName, string jsonContent)
    {
        using var connection = _connectionFactory.CreateConnection();
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
