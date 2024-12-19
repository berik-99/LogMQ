using LogMQ.Storage.Contracts;

namespace LogMQ.Services.Broker.Worker;

internal class TestDbReaderService(ILogger<TestDbReaderService> logger, ILogStorage storage) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var messages = await storage.GetLogMessages(DateTimeOffset.Now.Add(-TimeSpan.FromSeconds(30)), DateTimeOffset.Now, 100);
            logger.LogInformation("Retrieved {Count} messages from storage.", messages.Count);
            await Task.Delay(500, stoppingToken);
        }
    }
}
