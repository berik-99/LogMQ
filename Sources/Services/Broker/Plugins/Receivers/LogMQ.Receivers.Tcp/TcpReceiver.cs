using LightTcp;
using LogMQ.Core;
using LogMQ.Receivers.Contracts;
using Microsoft.Extensions.Logging;
using static LogMQ.Providers.TcpProvider;

namespace LogMQ.Receivers;

/// <summary>
/// Represents a LogMQ Receiver that leverages Socket TCP as the transport mechanism for messages sent by the compatible LogMQ TCP Provider.
/// </summary>
/// <param name="logger">An instance of <see cref="ILogger{TCategoryName}"/> used for logging events within the receiver.</param>
/// <param name="storage">An implementation of <see cref="ILogStorageWriter"/> used for storing received log messages.</param>
public class TcpReceiver(ILogger<TcpReceiver> logger, ILogStorage storage) : LogReceiverBase(storage)
{
    private readonly string tcpHost = DefaultTcpHost;
    private readonly int tcpPort = DefaultTcpPort;

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Initializing TCP Receiver...");
        using LightTcpServer tcpServer = new(tcpHost, tcpPort);
        tcpServer.MessageReceived += async (_, e) => await MessageReceived(e);
        tcpServer.Start();
        logger.LogInformation("Initialized TCP Receiver at {Host}:{Port}", tcpHost, tcpPort);
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(100, stoppingToken);
        }
    }

    /// <summary>
    /// Processes the received log message, deserializes it, and writes it to storage.
    /// </summary>
    /// <param name="data">The message data.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task MessageReceived(byte[] data)
    {
        LogMessage logMessage = LogMessage.Deserialize(data);
        await Storage.WriteLogMessageAsync(logMessage);
    }
}
