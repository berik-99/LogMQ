using LogMQ.Core;
using LogMQ.Providers.Contracts;
using System.Text;
using WatsonTcp;

namespace LogMQ.Providers;

/// <summary>
/// A Socket Tcp log provider for sending log messages to a LogMQ Tcp Broker.
/// </summary>
public sealed class TcpProvider : ILogProvider, IDisposable
{
	/// <summary>
	/// Default TCP host address used for the connection.
	/// </summary>
	public const string DefaultTcpHost = "localhost";

	/// <summary>
	/// Default TCP port used for the connection.
	/// </summary>
	public const int DefaultTcpPort = 5563;

	/// <summary>
	/// Default ping message sent to the broker for connection validation.
	/// </summary>
	public const string DefaultTcpPingMsg = "PING";

	/// <summary>
	/// Default pong message expected from the broker as a response to the ping.
	/// </summary>
	public const string DefaultTcpPongMsg = "PONG";

	private readonly WatsonTcpClient tcpClient;

	/// <inheritdoc />
	public IFormatProvider FormatProvider { get; }

	/// <inheritdoc />
	public IFallbackLogProvider FallbackLogger { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="TcpProvider"/> class.
	/// Establishes a TCP connection to the specified host and port.
	/// </summary>
	/// <param name="formatProvider">The format provider for log message formatting.</param>
	/// <param name="host">The TCP host address of the LogMQ Broker.</param>
	/// <param name="port">The TCP port of the LogMQ Broker.</param>
	/// <param name="fallbackLogger">The fallback logger for error handling and fallback logging.</param>
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
		catch (Exception ex)
		{
			Exception baseException = ex is AggregateException aex ? aex.GetBaseException() : ex;
			FallbackLogger.WriteError($"An error occurred during {nameof(TcpProvider)} initialization", baseException);
		}
	}

	/// <summary>
	/// Sends a ping message to the broker to validate the connection.
	/// </summary>
	/// <returns>A task that represents the asynchronous ping operation.</returns>
	/// <exception cref="InvalidOperationException">Thrown when the broker returns an invalid pong message or the operation fails.</exception>
	private async Task Ping()
	{
		try
		{
			byte[] message = Encoding.ASCII.GetBytes(DefaultTcpPingMsg);
			var res = await tcpClient.SendAndWaitAsync(2000, message);
			string pong = Encoding.UTF8.GetString(res.Data);
			if (pong != DefaultTcpPongMsg)
				throw new InvalidOperationException("Invalid pong message from broker");
		}
		catch (Exception ex)
		{
			FallbackLogger.WriteError("Error occurred during the Ping operation", ex);
			throw new InvalidOperationException("Ping operation failed", ex);
		}
	}

	/// <summary>
	/// Asynchronously sends a log message to the LogMQ Broker.
	/// </summary>
	/// <param name="message">The log message to be sent.</param>
	/// <returns>A task that represents the asynchronous write operation.</returns>
	/// <exception cref="InvalidOperationException">Thrown when the log message fails to send.</exception>
	private async Task WriteAsync(LogMessage message)
	{
		var bin = message.Serialize();
		if (!await tcpClient.SendAsync(bin))
			throw new InvalidOperationException("Failed to send log message to LogMQ Broker");
	}

	/// <inheritdoc />
	public void Write(LogMessage message)
	{
		try
		{
			WriteAsync(message).Wait();
		}
		catch (Exception ex)
		{
			Exception baseException = ex is AggregateException aex ? aex.GetBaseException() : ex;
			FallbackLogger.WriteError("Error occurred while writing log to LogMQ Broker", baseException);
			FallbackLogger.WriteFallback(message);
		}
	}

	/// <summary>
	/// Releases the resources used by the <see cref="TcpProvider"/> class.
	/// Disconnects the TCP client and disposes of its resources.
	/// </summary>
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
