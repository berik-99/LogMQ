using LogMQ.Messages;
using LogMQ.Plugins.Receivers.Contracts;
using LogMQ.Plugins.Storage.Contracts;
using Microsoft.Extensions.Logging;
using Msmq.NetCore.Messaging;
using System.Runtime.Versioning;

namespace LogMQ.Plugins.Receivers;

[SupportedOSPlatform("windows")]
public class MsmqReceiver(ILogger<MsmqReceiver> logger, ILogMQStorage rdb) : LogMQReceiverBase(logger, rdb)
{
	private readonly string queuePath = @".\Private$\LogMQ_Queue";

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		logger.LogInformation("Init MSMQ Receiver");
		if (!MessageQueue.Exists(queuePath))
			MessageQueue.Create(queuePath);
		using MessageQueue queue = new(queuePath);
		while (!stoppingToken.IsCancellationRequested)
		{
			await Task.Run(async () =>
			{
				Message message = queue.Receive();
				var stream = message.BodyStream;
				var logMessage = LogMessage.Deserialize(stream);
				await rdb.WriteLogMessage(logMessage);
			}, stoppingToken);
		}
	}
}