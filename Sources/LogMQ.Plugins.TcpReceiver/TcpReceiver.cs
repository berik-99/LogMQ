using LogMQ.Contracts;
using LogMQ.Plugins.Receivers.Contracts;
using LogMQ.Plugins.Storage.Contracts;
using Microsoft.Extensions.Logging;
using System.Text;
using WatsonTcp;
using static LogMQ.Providers.TcpProvider;

namespace LogMQ.Plugins.Receivers;

/// <summary>
/// Represents a LogMQ Receiver that leverages Socket TCP as the transport mechanism for messages sent by the compatible LogMQ TCP Provider.
/// </summary>
/// <param name="logger">An instance of <see cref="ILogger{TCategoryName}"/> used for logging events within the receiver.</param>
/// <param name="storage">An implementation of <see cref="ILogStorage"/> used for storing received log messages.</param>
public class TcpReceiver(ILogger<TcpReceiver> logger, ILogStorage storage) : LogReceiverBase(storage)
{
	private readonly string tcpHost = DefaultTcpHost;
	private readonly int tcpPort = DefaultTcpPort;

	/// <inheritdoc />
	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		logger.LogInformation("Init Tcp Receiver");
		using WatsonTcpServer tcpServer = new(tcpHost, tcpPort);
		tcpServer.Events.MessageReceived += async (s, e) => await MessageReceived(e);
		tcpServer.Callbacks.SyncRequestReceivedAsync = SyncMessageReceived;
		tcpServer.Start();
		while (!stoppingToken.IsCancellationRequested)
		{
			await Task.Delay(100, stoppingToken);
		}
	}

	/// <summary>
	/// Handles sync requests from clients, responding with a "pong" message.
	/// </summary>
	/// <param name="request">The sync request from the client.</param>
	/// <returns>A task representing the response to the sync request.</returns>
	private Task<SyncResponse> SyncMessageReceived(SyncRequest request)
	{
		byte[] message = Encoding.ASCII.GetBytes(DefaultTcpPongMsg);
		string ping = Encoding.UTF8.GetString(request.Data);
		if (ping != DefaultTcpPingMsg)
			throw new InvalidOperationException("Invalid ping message from client");
		return Task.FromResult(new SyncResponse(request, message));
	}

	/// <summary>
	/// Processes the received log message, deserializes it, and writes it to storage.
	/// </summary>
	/// <param name="e">The event arguments containing the message data.</param>
	/// <returns>A task representing the asynchronous operation.</returns>
	private async Task MessageReceived(MessageReceivedEventArgs e)
	{
		await using MemoryStream stream = new(e.Data);
		var logMessage = LogMessage.Deserialize(stream);
		await Storage.WriteLogMessage(logMessage);
	}
}
