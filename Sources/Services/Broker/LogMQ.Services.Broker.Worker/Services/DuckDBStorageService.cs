using Dapper;
using DuckDB.NET.Data;
using LogMQ.Core;
using LogMQ.Services.Shared.LogManager;
using ProtoBuf;

namespace LogMQ.Services.Broker.Worker.Services;

public class DuckDBStorageService(ILogger<DuckDBStorageService> logger, DuckDBStorageConfiguration config) : Receivers.Contracts.ILogStorage, ILogService
{
    public Task<List<string>> GetLogApplications()
    {
        logger.LogInformation("Request received for: GetLogApplications");
        return Task.FromResult<List<string>>(null);
    }

    public Task<List<LogMessage>> GetLogsAsync(LogFilter filter)
    {
        logger.LogInformation("Request received for: GetLogsAsync");
        return Task.FromResult<List<LogMessage>>(null);
    }

    public async Task<Wrapper<long>> GetTotalLogsCountAsync(Wrapper<string> applicationName)
    {
        await using DuckDBConnection conn = new($"Data Source={config.DbPath}");
        await conn.OpenAsync();
        int res = await conn.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM LogMessages");
        return res;
    }

    public async Task WriteLogMessageAsync(LogMessage logMessage)
    {
        await using DuckDBConnection conn = new($"Data Source={config.DbPath}");
        await conn.OpenAsync();
        await conn.ExecuteAsync("""
            CREATE TABLE IF NOT EXISTS LogMessages (
            Guid UUID NOT NULL,
            Timestamp TIMETZ NOT NULL,
            LogLevel VARCHAR NOT NULL,
            Message TEXT NOT NULL,
            Metadata BLOB,
            ExceptionMessage TEXT,
            PRIMARY KEY (Guid, Timestamp));
            """, logMessage);

        await using MemoryStream stream = new();
        Serializer.Serialize(stream, logMessage.Metadata);
        await conn.ExecuteAsync("INSERT INTO LogMessages VALUES ($Guid, $Timestamp, $LogLevel, $Message, $Metadata, $ExceptionMessage)",
            new
            {
                logMessage.Guid,
                Timestamp = logMessage.Timestamp.ToDateTimeOffset(),
                logMessage.LogLevel,
                logMessage.Message,
                Metadata = stream,
                logMessage.ExceptionMessage
            }
        );
    }
}
