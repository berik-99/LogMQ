using LogMQ.Contracts;
using Msmq.NetCore.Messaging;
using System.Runtime.Versioning;

namespace LogMQ.Providers;

/// <summary>
/// Provides a log provider implementation for sending log messages to LogMQ broker using Microsoft Message Queuing (MSMQ).
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class MsmqProvider : ILogProvider, IDisposable
{
	/// <summary>
	/// The default path for the MSMQ queue used by this provider.
	/// </summary>
	public const string DefaultQueuePath = @".\Private$\LogMQ_Queue";

	/// <summary>
	/// The instance of the MSMQ message queue.
	/// </summary>
	private readonly MessageQueue queue;

	/// <inheritdoc />
	public IFormatProvider FormatProvider { get; }

	/// <inheritdoc />
	public IFallbackLogProvider FallbackLogger { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="MsmqProvider"/> class.
	/// </summary>
	/// <param name="formatProvider">An object that provides culture-specific formatting information.</param>
	/// <param name="queuePath">The path to the MSMQ queue to use for storing log messages.</param>
	/// <param name="fallbackLogger">The fallback logger to use in case of errors.</param>
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

	/// <inheritdoc />
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

	/// <summary>
	/// Disposes of the resources used by the <see cref="MsmqProvider"/> instance.
	/// </summary>
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
