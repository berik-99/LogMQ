using LogMQ.Messages;
using System.Text;
using WatsonTcp;

namespace LogMQ.Providers;

public sealed class TcpProvider : ILogProvider, IDisposable
{
	public const string DefaultTcpHost = "localhost";
	public const int DefaultTcpPort = 5563;
	public const string DefaultTcpPingMsg = "PING";
	public const string DefaultTcpPongMsg = "PONG";

	private readonly WatsonTcpClient tcpClient;
	public IFormatProvider FormatProvider { get; }
	public IFallbackLogProvider FallbackLogger { get; }

	public TcpProvider(IFormatProvider formatProvider, string host, int port, IFallbackLogProvider fallbackLogger)
	{
		try
		{
			FormatProvider = formatProvider;
			FallbackLogger = fallbackLogger;
			tcpClient = new WatsonTcpClient(host, port);
			tcpClient.Events.MessageReceived += (s, e) => { };
			tcpClient.Connect();
			Task.Run(Ping).Wait();
		}
		catch (AggregateException aex)
		{
			FallbackLogger.WriteError($"An aggregate exception occurred during {nameof(TcpProvider)} initialization", aex.GetBaseException());
		}
		catch (Exception ex)
		{
			FallbackLogger.WriteError($"An error occurred during {nameof(TcpProvider)} initialization", ex);
		}
	}

	private async Task Ping()
	{
		try
		{
			byte[] message = Encoding.ASCII.GetBytes("PING");
			var res = await tcpClient.SendAndWaitAsync(2000, message);
			string pong = Encoding.UTF8.GetString(res.Data);
			if (pong != "PONG")
				throw new InvalidOperationException("Unexpected response from LogMQ Broker: Expected 'PONG'");
		}
		catch (Exception ex)
		{
			FallbackLogger.WriteError("Error occurred during the Ping operation", ex);
			throw new InvalidOperationException("Ping operation failed", ex);
		}
	}

	private async Task WriteAsync(LogMessage message)
	{
		var bin = message.Serialize();
		if (!await tcpClient.SendAsync(bin))
			throw new InvalidOperationException("Failed to send log message to LogMQ Broker");
	}

	public void Write(LogMessage message)
	{
		try
		{
			WriteAsync(message).Wait();
		}
		catch (AggregateException aex)
		{
			FallbackLogger.WriteError("An aggregate exception occurred while writing log to LogMQ Broker", aex.GetBaseException());
			FallbackLogger.WriteFallback(message);
		}
		catch (Exception ex)
		{
			FallbackLogger.WriteError("Error occurred while writing log to LogMQ Broker", ex);
			FallbackLogger.WriteFallback(message);
		}
	}

	public void Dispose()
	{
		try
		{
			if (tcpClient?.Connected == true)
				tcpClient.Disconnect();
			tcpClient?.Dispose();
			GC.SuppressFinalize(this);
		}
		catch (Exception ex)
		{
			FallbackLogger.WriteError($"Error occurred during {nameof(TcpProvider)} disposal", ex);
		}
	}
}
