using LogMQ.Messages;
using LogMQ.Plugins.Receivers.Contracts;
using LogMQ.Plugins.Storage.Contracts;
using Microsoft.Extensions.Logging;
using Msmq.NetCore.Messaging;
using System.Runtime.Versioning;
using static LogMQ.Providers.MsmqProvider;

namespace LogMQ.Plugins.Receivers;

[SupportedOSPlatform("windows")]
public class MsmqReceiver(ILogger<MsmqReceiver> logger, ILogMQStorage storage) : LogMQReceiverBase(storage)
{
	private readonly string queuePath = DefaultQueuePath;

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
				await Storage.WriteLogMessage(logMessage);
			}, stoppingToken);
		}
	}
}