# BlobToSqlite

![.NET 8.0](https://img.shields.io/badge/.NET-8.0-purple)
![License](https://img.shields.io/badge/License-MIT-blue)

**BlobToSqlite** is a robust, high-performance tool designed to download data from Azure Blob Storage and import it into a local SQLite database. It is built with modern .NET 8 standards, adhering to SOLID principles and utilizing Dependency Injection for maximum maintainability and testability.

## 🚀 Features

- **Azure Blob Integration**: Seamlessly connects to Azure Blob Storage to fetch data files.
- **SQLite Export**: Efficiently parses and stores data into a local SQLite database.
- **Smart Filtering**:
  - **Default Mode**: Automatically processes data from the last 30 days.
  - **Specific Date Mode**: targeted processing for a specific date.
- **Modern Architecture**:
  - Built on **.NET 8**.
  - Uses **Microsoft.Extensions.Hosting** for lifecycle management.
  - Implements **Dependency Injection (DI)**.
  - Uses the **Options Pattern** for type-safe configuration.

## 📋 Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) installed on your machine.
- An active **Azure Storage Account** with a container holding your data.

## ⚙️ Configuration

The application uses `appsettings.json` for configuration. You can also use `appsettings.local.json` for local overrides (this file is git-ignored by default).

### 1. Configure Settings
Ensure your `appsettings.json` (or `appsettings.local.json`) looks like this:

```json
{
  "AzureStorage": {
    "ConnectionString": "YOUR_AZURE_STORAGE_CONNECTION_STRING",
    "ContainerName": "your-container-name"
  },
  "Database": {
    "ConnectionString": "Data Source=blobdata.db"
  }
}
```

> [!IMPORTANT]
> Never commit your actual secrets or connection strings to version control. Use `appsettings.local.json` or User Secrets for sensitive data.

## 🏃 Usage

You can run the tool directly from the command line.

### Default Mode (Last 30 Days)
Running without arguments will process blobs modified in the last 30 days.

```bash
dotnet run --project BlobToSqlite
```

### Specific Date Mode
To process blobs for a specific date, pass the date as an argument (format: `YYYY-MM-DD`).

```bash
dotnet run --project BlobToSqlite 2023-10-27
```

## 🏗️ Architecture

This project follows **Clean Architecture** and **SOLID** principles:

- **Core**:
  - `IBlobStorage`: Abstraction for blob storage operations.
  - `IBlobRepository`: Abstraction for database operations.
  - `IBlobPathParser`: Logic for parsing blob paths/filenames.
- **Infrastructure**:
  - `AzureBlobStorage`: Concrete implementation using Azure SDK.
  - `BlobRepository`: Concrete implementation using Dapper and SQLite.
- **Application**:
  - `BlobImportService`: Orchestrates the flow between storage and database.

## 🛠️ Development

To build the project:

```bash
dotnet build
```

To run tests:

```bash
dotnet test
```
