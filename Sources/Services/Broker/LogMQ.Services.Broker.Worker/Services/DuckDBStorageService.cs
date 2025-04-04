using System.Text;
using Dapper;
using DuckDB.NET.Data;
using LogMQ.Core;
using LogMQ.Receivers.Contracts;
using LogMQ.Services.Broker.Worker.Services.Models;
using LogMQ.Services.Shared.LogManager;
using LogMQ.Services.Shared.LogManager.Filters;

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
        await using DuckDBConnection conn = new(BuildDataSourceString(appId));
        await conn.OpenAsync();
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

    public async Task<Wrapper<ulong>> CountLogsAsync(SearchFilter filter)
    {
        Guid appId = await GetApplicationId(filter.ApplicationName);
        await using DuckDBConnection conn = new(BuildDataSourceString(appId));
        await conn.OpenAsync();
        StringBuilder str = new("SELECT COUNT(*) FROM LogMessages WHERE Timestamp BETWEEN $DateFrom AND $DateTo");
        if (filter.LogLevel != null)
            str.Append(" AND LogLevel = $LogLevel");
        if (filter.Count != null)
            str.Append(" LIMIT $Count");
        ulong res = await conn.ExecuteScalarAsync<ulong>(str.ToString(), new { DateFrom = filter.DateFrom.ToDateTimeOffset(), DateTo = filter.DateTo.ToDateTimeOffset(), filter.LogLevel, filter.Count });
        return res;
    }

    public async Task<Wrapper<ulong>> ClearLogsAsync(ClearFilter filter)
    {
        Guid appId = await GetApplicationId(filter.ApplicationName);
        //if no date provided, will delete entire file and remove app from app list, otherwise will delete only the logs
        ulong count = 0;
        if (filter.OlderThan == null)
        {
            count = await CountLogsAsync(new SearchFilter { ApplicationName = filter.ApplicationName, DateFrom = UniversalDateTime.MinValue, DateTo = filter.OlderThan ?? UniversalDateTime.Now });
            File.Delete(BuildDataSourceString(appId, true));
            await using DuckDBConnection conn = new(BuildDataSourceString(Guid.Empty));
            await conn.OpenAsync();
            await conn.ExecuteAsync("DELETE FROM Applications WHERE Guid = $Guid", new { Guid = appId });
        }
        else
        {
            await using DuckDBConnection conn = new(BuildDataSourceString(appId));
            await conn.OpenAsync();
            count = (ulong)await conn.ExecuteAsync("DELETE FROM LogMessages WHERE Timestamp <= $OlderThan", new { filter.OlderThan });
        }
        return count;
    }

    public async Task<Wrapper<ulong>> MergeLogsAsync(MergeFilter filter)
    {
        Guid sourceAppId = await GetApplicationId(filter.SourceApplicationName);
        Guid targetAppId = await GetApplicationId(filter.TargetApplicationName);

        string dumpPath = $"./{sourceAppId}.tmp.parquet";
        await using DuckDBConnection sourceConn = new(BuildDataSourceString(sourceAppId));
        await sourceConn.OpenAsync();
        await sourceConn.ExecuteAsync($"COPY LogMessages TO '{dumpPath}' (FORMAT PARQUET)");
        await sourceConn.CloseAsync();
        await sourceConn.DisposeAsync();

        await using DuckDBConnection targetConn = new(BuildDataSourceString(targetAppId));
        await targetConn.OpenAsync();
        ulong count = (ulong)await targetConn.ExecuteAsync($"INSERT INTO LogMessages SELECT * FROM read_parquet('{dumpPath}')");
        //If everything is ok, delete the dump file and the source database.
        if (count > 0)
        {
            File.Delete(dumpPath);
            File.Delete(BuildDataSourceString(sourceAppId, true));
            await using DuckDBConnection conn = new(BuildDataSourceString(Guid.Empty));
            await conn.OpenAsync();
            await conn.ExecuteAsync("DELETE FROM Applications WHERE Guid = $Guid", new { Guid = sourceAppId });
        }
        return count;
    }

    public async Task WriteLogMessageAsync(LogMessage logMessage)
    {
        Guid appId = await GetApplicationId(logMessage.Application.Name, false);
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

    private async Task<Guid> GetApplicationId(string appName, bool throwIfNull = true)
    {
        await using DuckDBConnection conn = new(BuildDataSourceString(Guid.Empty));
        await conn.OpenAsync();
        Guid res = await conn.ExecuteScalarAsync<Guid>("SELECT Guid FROM Applications WHERE Name = $appName", new { appName });
        if (throwIfNull && res == Guid.Empty)
            throw new InvalidOperationException($"Application '{appName}' not found");
        return res;
    }

    private string BuildDataSourceString(Guid appId, bool pathOnly = false) => $"{(pathOnly ? "" : "Data Source=")}{Path.Join(config.DatabaseFolderPath, appId.ToString("N").ToUpperInvariant())}";
}
