using System.Threading.Channels;
using Dapper;
using DuckDB.NET.Data;
using LogMQ.Core;

namespace LogMQ.Services.Broker.Worker.Services;

internal class MessageWriterProcessor(RepositoryConfiguration config, ILogger<MessageWriterProcessor> logger, Channel<LogMessage> channel) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (await channel.Reader.WaitToReadAsync(stoppingToken))
        {
            LogMessage logMessage = await channel.Reader.ReadAsync(stoppingToken);
            logger.LogInformation("Received message from channel");
            await WriteLogMessageAsync(logMessage);
        }
    }

    private async Task WriteLogMessageAsync(LogMessage logMessage)
    {
        try
        {
            Guid appId = await config.GetApplicationId(logMessage.Application.Name, false);
            if (appId == Guid.Empty)
            {
                appId = Guid.NewGuid();
                await using DuckDBConnection rootConn = new(config.BuildDataSourceString(Guid.Empty));
                await rootConn.ExecuteAsync("INSERT INTO Applications VALUES ($Guid, $Name, $Category)",
                    new { Guid = appId, logMessage.Application.Name, logMessage.Application.Category });
            }

            await using DuckDBConnection conn = new(config.BuildDataSourceString(appId));
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
        catch (Exception ex)
        {
            logger.LogError(ex, "Cannot write message in LogMQ broker");
        }
    }
}
