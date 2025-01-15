using LogMQ.Core;
using LogMQ.Storage.Contracts;

namespace LogMQ.Services.Broker.Worker;

internal class TestDbReaderService(ILogger<TestDbReaderService> logger, ILogStorage storage) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            const string appName = "SerilogTestConsole";
            var count = await storage.GetLogsCountAsync(appName);
            logger.LogInformation("Retrieved {Count} messages for application '{App}'.", count, appName);

            var last = await storage.GetLogsAsync(new LogFilter() { ApplicationName = appName, Count = 30 });
            for (int i = 0; i < last.Count; i++)
            {
                LogMessage log = last[i];
                logger.LogInformation("Retrieved #{Index} TS: {Timestamp}; message: {Message}", i, log.Timestamp.ToString("yyyy/MM/dd HH:mm:ss.fff"), log.Message);
            }
            await Task.Delay(500, stoppingToken);
        }
    }
}
