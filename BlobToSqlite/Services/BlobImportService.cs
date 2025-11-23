using BlobToSqlite.Services;

namespace BlobToSqlite.Services;

public class BlobImportService
{
    private readonly IBlobStorage _blobStorage;
    private readonly DatabaseService _dbService;

    public BlobImportService(IBlobStorage blobStorage, DatabaseService dbService)
    {
        _blobStorage = blobStorage;
        _dbService = dbService;
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
                // Parse Path: partner/eventType/date/filename
                var parts = blob.Name.Split('/');
                if (parts.Length < 4)
                {
                    // Console.WriteLine($"Skipped {blob.Name} (Invalid path structure)."); // Reduce noise
                    continue;
                }

                string partnerName = parts[0];
                string eventType = parts[1];
                string dateString = parts[2];
                string fileName = parts[parts.Length - 1];

                if (!DateTime.TryParse(dateString, out DateTime eventDateTime))
                {
                    Console.WriteLine($"Warning: Could not parse date '{dateString}' for {blob.Name}. Skipping.");
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
                    int partnerId = await _dbService.GetOrCreatePartnerIdAsync(partnerName);
                    await _dbService.InsertBlobDataAsync(partnerId, eventType, eventDateTime, fileName, content);
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
