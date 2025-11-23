using Azure.Storage.Blobs.Models;

namespace BlobToSqlite.Services;

public interface IBlobStorage
{
    /// <summary>
    /// Lists all blobs in the container recursively.
    /// </summary>
    /// <param name="containerName">Name of the container.</param>
    /// <returns>Enumerable of BlobItems.</returns>
    IAsyncEnumerable<BlobItem> ListBlobsRecursiveAsync(string containerName);

    /// <summary>
    /// Downloads the content of a blob as a string.
    /// </summary>
    /// <param name="containerName">Name of the container.</param>
    /// <param name="blobName">Name of the blob.</param>
    /// <returns>Content of the blob.</returns>
    Task<string> DownloadBlobTextAsync(string containerName, string blobName);
}
