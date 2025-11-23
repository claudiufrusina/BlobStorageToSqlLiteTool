# Blob to SQLite Downloader - Walkthrough

## Overview
This console application connects to Azure Blob Storage, scans a container for JSON files, and imports their content into a local SQLite database (`blobs.db`).

## Prerequisites
- .NET 8 SDK
- Azure Storage Account (Connection String)
- A container with JSON files

## How to Run

### 1. Configure Settings
Open `appsettings.json` (or create `appsettings.local.json` to override) and set your Azure details:

```json
{
  "AzureStorage": {
    "ConnectionString": "your_connection_string_here",
    "ContainerName": "your_container_name"
  }
}
```

### 2. Run the Application

**Default (Last 30 Days):**
```bash
dotnet run
```

**Specific Date:**
```bash
dotnet run -- 2025-11-23
```

### 3. Verify Results
The application will create a `blobs.db` file in the same directory. You can inspect it using any SQLite viewer.

## Code Structure
- **Services/IBlobStorage.cs**: Interface for blob operations.
- **Services/AzureBlobStorage.cs**: Implementation using `Azure.Storage.Blobs`.
- **Services/DatabaseService.cs**: Handles SQLite operations using `Dapper`.
    - Creates `Partners` and `BlobData` tables.
    - Normalizes partner names.
- **Program.cs**: Main entry point.
    - Parses blob paths: `partner/eventType/date/filename`.

## Database Schema
- **Partners**: `Id`, `Name`
- **BlobData**: `Id`, `PartnerId`, `EventType`, `EventDateTime`, `FileName`, `JsonContent`, `ImportedAt`
