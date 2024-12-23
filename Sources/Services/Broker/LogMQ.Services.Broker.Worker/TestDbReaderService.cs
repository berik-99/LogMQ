using LogMQ.Core;
using LogMQ.Storage.Contracts;

namespace LogMQ.Services.Broker.Worker;

internal class TestDbReaderService(ILogger<TestDbReaderService> logger, ILogStorage storage) : BackgroundService
{
	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		while (!stoppingToken.IsCancellationRequested)
		{
			var now = DateTimeOffset.Now;
			var messages = await storage.GetLogsByFilterAsync(new LogFilter { TimeFrom = now.DateTime.Add(-TimeSpan.FromSeconds(30)), TimeTo = now.DateTime, TimeOffset = now.Offset });
			logger.LogInformation("Retrieved {Count} messages from storage.", messages.Count);
			await Task.Delay(500, stoppingToken);
		}
	}
}
