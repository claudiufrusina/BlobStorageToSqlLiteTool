using Azure.Storage.Blobs.Models;
using BlobToSqlite.Services;

namespace BlobToSqlite.Tests;

public class LocalFileBlobStorage : IBlobStorage
{
    private readonly string _basePath;

    public LocalFileBlobStorage(string basePath)
    {
        _basePath = basePath;
    }

    public async IAsyncEnumerable<BlobItem> ListBlobsRecursiveAsync(string containerName)
    {
        // In this simulation, containerName is ignored or treated as a subfolder if needed.
        // We assume _basePath points to the root "Input" folder.
        
        var directoryInfo = new DirectoryInfo(_basePath);
        if (!directoryInfo.Exists)
        {
            yield break;
        }

        var files = directoryInfo.GetFiles("*", SearchOption.AllDirectories);

        foreach (var file in files)
        {
            // Create a relative path that mimics the blob name: Partner/Event/Date/File.json
            var relativePath = Path.GetRelativePath(_basePath, file.FullName).Replace("\\", "/");
            
            // Create a mock BlobItem (BlobItem is sealed/complex to mock directly with Moq often, 
            // but we can instantiate it via reflection or just return a wrapper if we changed the interface.
            // However, BlobItemModelFactory is the official way to create instances for testing.)
            
            var blobItem = Azure.Storage.Blobs.Models.BlobsModelFactory.BlobItem(name: relativePath, properties: null);
            
            yield return blobItem;
        }
        await Task.CompletedTask;
    }

    public async Task<string> DownloadBlobTextAsync(string containerName, string blobName)
    {
        var fullPath = Path.Combine(_basePath, blobName);
        if (File.Exists(fullPath))
        {
            return await File.ReadAllTextAsync(fullPath);
        }
        return null;
    }
}
