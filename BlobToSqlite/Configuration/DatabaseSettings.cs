namespace BlobToSqlite.Configuration;

public class DatabaseSettings
{
    public string ConnectionString { get; set; } = string.Empty;
    public string Provider { get; set; } = "Sqlite";
}
