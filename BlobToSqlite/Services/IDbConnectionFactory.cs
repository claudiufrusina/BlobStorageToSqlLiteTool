using System.Data;
using BlobToSqlite.Configuration;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;

namespace BlobToSqlite.Services;

public interface IDbConnectionFactory
{
    IDbConnection CreateConnection();
    string GetProviderName();
}

public class DbConnectionFactory : IDbConnectionFactory
{
    private readonly DatabaseSettings _settings;

    public DbConnectionFactory(IOptions<DatabaseSettings> settings)
    {
        _settings = settings.Value;
    }

    public IDbConnection CreateConnection()
    {
        return _settings.Provider.ToLowerInvariant() switch
        {
            "sqlserver" => new SqlConnection(_settings.ConnectionString),
            "sqlite" => new SqliteConnection(_settings.ConnectionString),
            _ => throw new ArgumentException($"Unsupported provider: {_settings.Provider}")
        };
    }

    public string GetProviderName()
    {
        return _settings.Provider;
    }
}
