namespace BlobToSqlite.Services;

public interface IBlobPathParser
{
    bool TryParse(string blobName, out string partnerName, out string eventType, out DateTime eventDateTime, out string fileName);
}

public class StandardBlobPathParser : IBlobPathParser
{
    public bool TryParse(string blobName, out string partnerName, out string eventType, out DateTime eventDateTime, out string fileName)
    {
        partnerName = string.Empty;
        eventType = string.Empty;
        eventDateTime = DateTime.MinValue;
        fileName = string.Empty;

        var parts = blobName.Split('/');
        if (parts.Length < 4)
        {
            return false;
        }

        partnerName = parts[0];
        eventType = parts[1];
        var dateString = parts[2];
        fileName = parts[parts.Length - 1];

        return DateTime.TryParse(dateString, out eventDateTime);
    }
}
