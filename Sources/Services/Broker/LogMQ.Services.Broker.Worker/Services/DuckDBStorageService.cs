using System.Text;
using Dapper;
using DuckDB.NET.Data;
using LogMQ.Core;
using LogMQ.Receivers.Contracts;
using LogMQ.Services.Shared.LogManager;
using LogMQ.Services.Shared.LogManager.Filters;
using LogLevel = LogMQ.Core.LogLevel;

namespace LogMQ.Services.Broker.Worker.Services;

public class DuckDBStorageService : ILogStorage, ILogGrpcService
{
    private readonly DuckDBStorageConfiguration config;
    private readonly ILogger logger;

    public DuckDBStorageService(ILogger<DuckDBStorageService> logger, DuckDBStorageConfiguration config)
    {
        logger.LogInformation("Initializing database storage...");
        this.config = config;
        this.logger = logger;
        Directory.CreateDirectory(config.DatabaseFolderPath);
        using DuckDBConnection conn = new(BuildDataSourceString(Guid.Empty));
        conn.Open();
        conn.Execute(@"
        CREATE TABLE IF NOT EXISTS Applications (
            Guid UUID NOT NULL PRIMARY KEY,
            Name TEXT NOT NULL UNIQUE,
            Category TEXT NOT NULL DEFAULT 'Generic');
        ");
        logger.LogInformation("Initalized database storage in {Path}", config.DatabaseFolderPath);
    }

    public async Task<List<string>> GetLogApplications()
    {
        await using DuckDBConnection conn = new(BuildDataSourceString(Guid.Empty));
        await conn.OpenAsync();
        IEnumerable<string> res = await conn.QueryAsync<string>("SELECT Name FROM Applications");
        return res.ToList();
    }

    public async Task<List<LogMessage>> GetLogsAsync(SearchFilter filter)
    {
        Guid appId = await GetApplicationId(filter.ApplicationName);
        if (appId == Guid.Empty)
            throw new ArgumentException("Application not found", nameof(filter));
        await using DuckDBConnection conn = new(BuildDataSourceString(appId));
        await conn.OpenAsync();

        if (filter.DateTo - filter.DateFrom > TimeSpan.FromDays(10) && filter.Count is null)
            throw new InvalidOperationException("Cannot retrive logs fot time range grater than 10 days whithout providing a count limit");

        StringBuilder str = new("SELECT * FROM (SELECT * FROM LogMessages WHERE Timestamp BETWEEN $DateFrom AND $DateTo");
        if (filter.LogLevel != null)
            str.Append(" AND LogLevel = $LogLevel");
        str.Append(" ORDER BY Timestamp DESC");
        if (filter.Count != null)
            str.Append(" LIMIT $Count");
        str.Append(") ORDER BY Timestamp");

        IEnumerable<QueryLogMessage> res = await conn.QueryAsync<QueryLogMessage>(str.ToString(), new { DateFrom = filter.DateFrom.ToDateTimeOffset(), DateTo = filter.DateTo.ToDateTimeOffset(), filter.LogLevel, filter.Count });
        return res.ToList().ConvertAll(x => x.ToLogMessage());
    }

    public async Task<Wrapper<long>> GetTotalLogsCountAsync(Wrapper<string> applicationName)
    {
        Guid appId = await GetApplicationId(applicationName);
        if (appId == Guid.Empty)
            throw new ArgumentException("Application not found", nameof(applicationName));
        await using DuckDBConnection conn = new(BuildDataSourceString(appId));
        await conn.OpenAsync();
        int res = await conn.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM LogMessages");
        return res;
    }

    public async Task WriteLogMessageAsync(LogMessage logMessage)
    {
        Guid appId = await GetApplicationId(logMessage.Application.Name);
        if (appId == Guid.Empty)
        {
            appId = Guid.NewGuid();
            await using DuckDBConnection rootConn = new(BuildDataSourceString(Guid.Empty));
            await rootConn.ExecuteAsync("INSERT INTO Applications VALUES ($Guid, $Name, $Category)",
                new { Guid = appId, logMessage.Application.Name, logMessage.Application.Category });
        }

        await using DuckDBConnection conn = new(BuildDataSourceString(appId));
        await conn.OpenAsync();
        await conn.ExecuteAsync(@"
        CREATE TABLE IF NOT EXISTS LogMessages (
            Guid UUID NOT NULL,
            Timestamp TIMESTAMPTZ NOT NULL,
            LogLevel VARCHAR NOT NULL,
            Message TEXT NOT NULL,
            Metadata BLOB,
            Application BLOB,
            ExceptionMessage TEXT,
            PRIMARY KEY (Guid, Timestamp));
        ");

        int res = await conn.ExecuteAsync("INSERT OR IGNORE INTO LogMessages VALUES ($Guid, $Timestamp, $LogLevel, $Message, $Metadata, $Application, $ExceptionMessage)",
            new
            {
                logMessage.Guid,
                Timestamp = logMessage.Timestamp.ToDateTimeOffset(),
                logMessage.LogLevel,
                logMessage.Message,
                Metadata = logMessage.Metadata.Serialize(),
                Application = logMessage.Application.Serialize(),
                logMessage.ExceptionMessage
            }
        );
        logger.LogInformation("Inserted {Count} messages in storage", res);
    }

    private async Task<Guid> GetApplicationId(string AppName)
    {
        await using DuckDBConnection conn = new(BuildDataSourceString(Guid.Empty));
        await conn.OpenAsync();
        Guid res = await conn.ExecuteScalarAsync<Guid>("SELECT Guid FROM Applications WHERE Name = $AppName", new { AppName });
        return res;
    }

    private string BuildDataSourceString(Guid appId) => $"Data Source={Path.Join(config.DatabaseFolderPath, appId.ToString("N").ToUpperInvariant())}";
}

public class QueryLogMessage
{
    public Guid Guid { get; set; }

    public DateTimeOffset Timestamp { get; set; }

    public LogLevel LogLevel { get; set; }

    public string Message { get; set; }

    private byte[] metadata;
    public UnmanagedMemoryStream Metadata
    {
        set
        {
            using MemoryStream tmpStream = new();
            value.CopyTo(tmpStream);
            metadata = tmpStream.ToArray();
        }
    }

    private byte[] application;
    public UnmanagedMemoryStream Application
    {
        set
        {
            using MemoryStream tmpStream = new();
            value.CopyTo(tmpStream);
            application = tmpStream.ToArray();
        }
    }

    public string ExceptionMessage { get; set; }

    public LogMessage ToLogMessage()
    {
        return new LogMessage
        {
            Guid = Guid,
            LogLevel = LogLevel,
            Message = Message,
            Timestamp = Timestamp,
            Application = LogApplication.Deserialize(application),
            Metadata = LogMetadata.Deserialize(metadata),
            ExceptionMessage = ExceptionMessage
        };
    }
}