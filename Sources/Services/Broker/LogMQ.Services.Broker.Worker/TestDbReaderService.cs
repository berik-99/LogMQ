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
            long count = await storage.GetTotalLogsCountAsync(appName);
            logger.LogInformation("Retrieved {Count} messages for application '{App}'.", count, appName);

            var now = UniversalDateTime.Now;
            List<LogMessage> last = await storage.GetLogsAsync(new LogFilter() { ApplicationName = appName, Count = 100, TimeFrom = now.AddSeconds(-30), TimeTo = now });
            //for (int i = 0; i < last.Count; i++)
            //{
            //    LogMessage log = last[i];
            //    logger.LogInformation("Retrieved #{Index} TS: {Timestamp}; message: {Message}", i, log.Timestamp.ToString("yyyy/MM/dd HH:mm:ss.fff"), log.Message);
            //}

            logger.LogInformation("Retrieved {Count} messages for application '{App}' in last 30 seconds.", last.Count, appName);
            await Task.Delay(1000, stoppingToken);
        }
    }
}
