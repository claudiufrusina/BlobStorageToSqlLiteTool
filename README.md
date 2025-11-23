# BlobStorageToSqlLiteTool Solution

This solution contains a set of tools and libraries designed to facilitate the extraction of data from Azure Blob Storage and its importation into a local SQLite database. It is engineered with a focus on performance, maintainability, and modern .NET practices.

## 📂 Projects

The solution consists of the following projects:

### 1. [BlobToSqlite](./BlobToSqlite/README.md)
The core console application that performs the actual data extraction and transformation.
- **Key Features**: Azure Blob integration, SQLite export, Date-based filtering.
- **Tech Stack**: .NET 8, Dapper, Microsoft.Extensions.Hosting.

### 2. **BlobToSqlite.Tests**
Contains unit and integration tests to ensure the reliability and correctness of the application logic.

## 🚀 Getting Started

To get started with the solution, ensure you have the [.NET 8.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) installed.

### Build the Solution

```bash
dotnet build BlobToSqliteSolution.sln
```

### Run Tests

```bash
dotnet test BlobToSqliteSolution.sln
```

## 📄 License

This project is licensed under the MIT License.
