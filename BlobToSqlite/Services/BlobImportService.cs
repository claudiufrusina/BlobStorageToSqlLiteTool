using BlobToSqlite.Services;

namespace BlobToSqlite.Services;

public class BlobImportService
{
    private readonly IBlobStorage _blobStorage;
    private readonly IBlobRepository _blobRepository;
    private readonly IBlobPathParser _pathParser;

    public BlobImportService(IBlobStorage blobStorage, IBlobRepository blobRepository, IBlobPathParser pathParser)
    {
        _blobStorage = blobStorage;
        _blobRepository = blobRepository;
        _pathParser = pathParser;
    }

    public async Task<int> RunImportAsync(string containerName, DateTime? specificDate, DateTime? minDate)
    {
        Console.WriteLine($"Scanning container '{containerName}'...");
        int count = 0;

        await foreach (var blob in _blobStorage.ListBlobsRecursiveAsync(containerName))
        {
            // Filter for JSON files
            if (blob.Name.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            {
                if (!_pathParser.TryParse(blob.Name, out string partnerName, out string eventType, out DateTime eventDateTime, out string fileName))
                {
                    // Console.WriteLine($"Skipped {blob.Name} (Invalid path structure)."); // Reduce noise
                    continue;
                }

                // Apply Date Filter
                if (specificDate.HasValue)
                {
                    if (eventDateTime.Date != specificDate.Value)
                    {
                        continue; // Skip if not matching specific date
                    }
                }
                else if (minDate.HasValue)
                {
                    if (eventDateTime.Date < minDate.Value)
                    {
                        continue; // Skip if older than min date
                    }
                }

                Console.Write($"Processing {blob.Name}... ");
                
                var content = await _blobStorage.DownloadBlobTextAsync(containerName, blob.Name);
                
                if (!string.IsNullOrWhiteSpace(content))
                {
                    int partnerId = await _blobRepository.GetOrCreatePartnerIdAsync(partnerName);
                    await _blobRepository.InsertBlobDataAsync(partnerId, eventType, eventDateTime, fileName, content);
                    Console.WriteLine("Done.");
                    count++;
                }
                else
                {
                    Console.WriteLine("Skipped (Empty).");
                }
            }
        }
        return count;
    }
}
