using LogMQ.Messages;
using LogMQ.Plugins.Receivers.Contracts;
using LogMQ.Plugins.Storage.Contracts;
using Microsoft.Extensions.Logging;
using System.Text;
using WatsonTcp;
using static LogMQ.Providers.TcpProvider;

namespace LogMQ.Plugins.Receivers;

public class TcpReceiver(ILogger<TcpReceiver> logger, ILogMQStorage storage) : LogMQReceiverBase(storage)
{

	private readonly string tcpHost = DefaultTcpHost;
	private readonly int tcpPort = DefaultTcpPort;

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

	private Task<SyncResponse> SyncMessageReceived(SyncRequest request)
	{
		byte[] message = Encoding.ASCII.GetBytes(DefaultTcpPongMsg);
		string ping = Encoding.UTF8.GetString(request.Data);
		if (ping != DefaultTcpPingMsg)
			throw new InvalidOperationException("Invalid ping message from client");
		return Task.FromResult(new SyncResponse(request, message));
	}


	private async Task MessageReceived(MessageReceivedEventArgs e)
	{
		await using MemoryStream stream = new(e.Data);
		var logMessage = LogMessage.Deserialize(stream);
		await Storage.WriteLogMessage(logMessage);
	}
}