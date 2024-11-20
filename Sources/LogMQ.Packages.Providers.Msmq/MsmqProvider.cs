using LogMQ.Messages;
using Msmq.NetCore.Messaging;
using System.Runtime.Versioning;

namespace LogMQ.Providers;

[SupportedOSPlatform("windows")]
public sealed class MsmqProvider : ILogProvider, IDisposable
{
	private readonly MessageQueue queue;
	public IFormatProvider FormatProvider { get; }
	public IFallbackLogProvider FallbackLogger { get; }

	public MsmqProvider(IFormatProvider formatProvider, string queuePath, IFallbackLogProvider fallbackLogger)
	{
		try
		{
			FormatProvider = formatProvider;
			FallbackLogger = fallbackLogger;
			if (!MessageQueue.Exists(queuePath))
				MessageQueue.Create(queuePath);
			queue = new MessageQueue(queuePath);
		}
		catch (Exception ex)
		{
			FallbackLogger.WriteError($"An error occurred during {nameof(MsmqProvider)} initialization", ex);
		}
	}

	public void Write(LogMessage message)
	{
		try
		{
			using MemoryStream stream = new();
			message.SerializeToStream(stream);
			Message queueMsg = new()
			{
				BodyStream = stream,
				Label = $"LogMQ_{message.Application.Name}_{message.Timestamp:yyyy-MM-ddTHH:mm:ss.fffZ}"
			};
			queue.Send(queueMsg);
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
			queue.Dispose();
			GC.SuppressFinalize(this);
		}
		catch (Exception ex)
		{
			FallbackLogger.WriteError($"Error occurred during {nameof(MsmqProvider)} disposal", ex);
		}
	}
}
