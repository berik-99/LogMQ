using System.Text;
using System.Threading.Channels;
using Dapper;
using DuckDB.NET.Data;
using LogMQ.Core;
using LogMQ.Receivers.Contracts;
using LogMQ.Services.Broker.Worker.Services.Models;
using LogMQ.Services.Shared.LogManager;
using LogMQ.Services.Shared.LogManager.Filters;

namespace LogMQ.Services.Broker.Worker.Services;

internal class RepositoryService : ILogStorage, ILogGrpcService
{
    private readonly RepositoryConfiguration config;
    private readonly ILogger logger;
    private readonly Channel<LogMessage> channel;

    public RepositoryService(ILogger<RepositoryService> logger, RepositoryConfiguration config, Channel<LogMessage> channel)
    {
        logger.LogInformation("Initializing database storage...");
        this.config = config;
        this.logger = logger;
        this.channel = channel;
        Directory.CreateDirectory(config.DatabaseFolderPath);
        using DuckDBConnection conn = new(config.BuildDataSourceString(Guid.Empty));
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
        await using DuckDBConnection conn = new(config.BuildDataSourceString(Guid.Empty));
        await conn.OpenAsync();
        IEnumerable<string> res = await conn.QueryAsync<string>("SELECT Name FROM Applications");
        return res.ToList();
    }

    public async Task<List<LogMessage>> GetLogsAsync(SearchFilter filter)
    {
        Guid appId = await config.GetApplicationId(filter.ApplicationName);
        await using DuckDBConnection conn = new(config.BuildDataSourceString(appId));
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
        Guid appId = await config.GetApplicationId(filter.ApplicationName);
        await using DuckDBConnection conn = new(config.BuildDataSourceString(appId));
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
        Guid appId = await config.GetApplicationId(filter.ApplicationName);
        //if no date provided, will delete entire file and remove app from app list, otherwise will delete only the logs
        ulong count = 0;
        if (filter.OlderThan == null)
        {
            count = await CountLogsAsync(new SearchFilter { ApplicationName = filter.ApplicationName, DateFrom = UniversalDateTime.MinValue, DateTo = filter.OlderThan ?? UniversalDateTime.Now });
            File.Delete(config.BuildDataSourceString(appId, true));
            await using DuckDBConnection conn = new(config.BuildDataSourceString(Guid.Empty));
            await conn.OpenAsync();
            await conn.ExecuteAsync("DELETE FROM Applications WHERE Guid = $Guid", new { Guid = appId });
        }
        else
        {
            await using DuckDBConnection conn = new(config.BuildDataSourceString(appId));
            await conn.OpenAsync();
            count = (ulong)await conn.ExecuteAsync("DELETE FROM LogMessages WHERE Timestamp <= $OlderThan", new { filter.OlderThan });
        }
        return count;
    }

    public async Task<Wrapper<ulong>> MergeLogsAsync(MergeFilter filter)
    {
        Guid sourceAppId = await config.GetApplicationId(filter.SourceApplicationName);
        Guid targetAppId = await config.GetApplicationId(filter.TargetApplicationName);

        string dumpPath = $"./{sourceAppId}.tmp.parquet";
        await using DuckDBConnection sourceConn = new(config.BuildDataSourceString(sourceAppId));
        await sourceConn.OpenAsync();
        await sourceConn.ExecuteAsync($"COPY LogMessages TO '{dumpPath}' (FORMAT PARQUET)");
        await sourceConn.CloseAsync();
        await sourceConn.DisposeAsync();

        await using DuckDBConnection targetConn = new(config.BuildDataSourceString(targetAppId));
        await targetConn.OpenAsync();
        ulong count = (ulong)await targetConn.ExecuteAsync($"INSERT INTO LogMessages SELECT * FROM read_parquet('{dumpPath}')");
        //If everything is ok, delete the dump file and the source database.
        if (count > 0)
        {
            File.Delete(dumpPath);
            File.Delete(config.BuildDataSourceString(sourceAppId, true));
            await using DuckDBConnection conn = new(config.BuildDataSourceString(Guid.Empty));
            await conn.OpenAsync();
            await conn.ExecuteAsync("DELETE FROM Applications WHERE Guid = $Guid", new { Guid = sourceAppId });
        }
        return count;
    }

    public async Task WriteLogMessageAsync(LogMessage logMessage)
    {
        await channel.Writer.WriteAsync(logMessage);
    }
}
