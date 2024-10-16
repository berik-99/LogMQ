using LogMQ.Broker.Services.InternalQueueServices;
using MSMQ.Messaging;

namespace LogMQ.Broker
{
	public class MSMQReader(ILogger<MSMQReader> logger, RocksDbService rdb) : BackgroundService
	{
		private readonly string queuePath = @".\Private$\LogMQ_Queue";

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			if (!MessageQueue.Exists(queuePath))
				MessageQueue.Create(queuePath);
			using MessageQueue queue = new(queuePath);
			queue.Formatter = new ActiveXMessageFormatter();
			while (!stoppingToken.IsCancellationRequested)
			{
				Message message = queue.Receive();
				var json = (string)message.Body;
				logger.LogInformation("Read message from MSMQ");
				await rdb.WriteLogMessage(Guid.NewGuid().ToString(), json);
				logger.LogInformation("Writed message to RockDB queue");
			}
		}
	}
}
