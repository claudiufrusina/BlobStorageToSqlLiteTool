using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace BlobToSqlite.Services;

public class AzureBlobStorage : IBlobStorage
{
    private readonly BlobServiceClient _blobServiceClient;

    public AzureBlobStorage(string connectionString)
    {
        _blobServiceClient = new BlobServiceClient(connectionString);
    }

    public async IAsyncEnumerable<BlobItem> ListBlobsRecursiveAsync(string containerName)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        // Ensure container exists
        if (!await containerClient.ExistsAsync())
        {
            yield break;
        }

        // List blobs hierarchically? No, flat listing is better for "scan every folder" 
        // as we just want all files.
        await foreach (var blob in containerClient.GetBlobsAsync())
        {
            yield return blob;
        }
    }

    public async Task<string> DownloadBlobTextAsync(string containerName, string blobName)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        var blobClient = containerClient.GetBlobClient(blobName);

        if (!await blobClient.ExistsAsync())
        {
            return null;
        }

        var response = await blobClient.DownloadContentAsync();
        return response.Value.Content.ToString();
    }
}
