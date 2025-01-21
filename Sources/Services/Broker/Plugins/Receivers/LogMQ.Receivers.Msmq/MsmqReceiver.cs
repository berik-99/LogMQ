using System.Runtime.Versioning;
using LogMQ.Core;
using LogMQ.Receivers.Contracts;
using LogMQ.Storage.Contracts;
using Microsoft.Extensions.Logging;
using Msmq.NetCore.Messaging;
using static LogMQ.Providers.MsmqProvider;

namespace LogMQ.Receivers;

/// <summary>
/// Represents a LogMQ Receiver that leverages Microsoft Message Queuing (MSMQ) as the transport mechanism for messages sent by the compatible LogMQ Provider.
/// </summary>
/// <remarks>
/// This receiver is supported only on Windows.
/// </remarks>
/// <param name="logger">An instance of <see cref="ILogger{TCategoryName}"/> for logging events.</param>
/// <param name="storage">An implementation of <see cref="ILogStorageWriter"/> for storing received log messages.</param>
[SupportedOSPlatform("windows")]
public class MsmqReceiver(ILogger<MsmqReceiver> logger, ILogStorage storage) : LogReceiverBase(storage)
{
    /// <summary>
    /// The path to the MSMQ queue being used.
    /// </summary>
    private readonly string queuePath = DefaultQueuePath;

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Initializing MSMQ Receiver...");
        if (!MessageQueue.Exists(queuePath))
            MessageQueue.Create(queuePath);
        using MessageQueue queue = new(queuePath);
        logger.LogInformation("Initialized MSMQ Receiver at {Path}", queuePath);
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Run(async () =>
            {
                Message message = queue.Receive();
                MemoryStream stream = new();
                message.BodyStream.CopyTo(stream);
                LogMessage logMessage = LogMessage.Deserialize(stream.ToArray());
                await Storage.WriteLogMessageAsync(logMessage);
            }, stoppingToken);
        }
    }
}