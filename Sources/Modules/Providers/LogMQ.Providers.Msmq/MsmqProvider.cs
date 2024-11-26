using LogMQ.Core;
using LogMQ.Providers.Contracts;
using Msmq.NetCore.Messaging;
using System.Runtime.Versioning;

namespace LogMQ.Providers;

/// <summary>
/// A Microsoft Message Queuing (MSMQ) log provider for sending log messages to the Msmq Receiver inside the LogMQ Broker.
/// This provider is supported only on Windows platforms.
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
    /// Establishes a connection to the specified MSMQ queue for storing log messages.
    /// </summary>
    /// <param name="formatProvider">
    /// An object that provides culture-specific formatting information for log messages.
    /// Cannot be null.
    /// </param>
    /// <param name="queuePath">
    /// The path to the MSMQ queue to use for storing log messages.
    /// If the specified queue does not exist, it will be created automatically.
    /// Cannot be null or empty.
    /// </param>
    /// <param name="fallbackLogger">
    /// The fallback logger to use for error handling and logging when MSMQ operations fail.
    /// Cannot be null.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="formatProvider"/>, <paramref name="queuePath"/>, or <paramref name="fallbackLogger"/> is null or empty.
    /// </exception>
    /// <remarks>
    /// This class requires the MSMQ service to be installed and running on the host system.
    /// </remarks>
    public MsmqProvider(IFormatProvider formatProvider, string queuePath, IFallbackLogProvider fallbackLogger)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(fallbackLogger);
            ArgumentNullException.ThrowIfNullOrWhiteSpace(queuePath);
            FormatProvider = formatProvider;
            FallbackLogger = fallbackLogger;
            if (!MessageQueue.Exists(queuePath))
                MessageQueue.Create(queuePath);
            queue = new MessageQueue(queuePath);
        }
        catch (Exception ex)
        {
            Exception baseException = ex is AggregateException aex ? aex.GetBaseException() : ex;
            FallbackLogger.WriteError($"Error occurred during {nameof(MsmqProvider)} initalization", baseException);
            throw;
        }
    }

    /// <inheritdoc />
    /// <summary>
    /// Sends a log message to the MSMQ queue.
    /// </summary>
    /// <param name="message">
    /// The log message to be sent to the MSMQ queue.
    /// </param>
    /// <exception cref="Exception">
    /// Thrown when an error occurs while sending the log message to the MSMQ queue.
    /// </exception>
    /// <remarks>
    /// If the send operation fails, the log message is forwarded to the fallback logger.
    /// </remarks>
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
            FallbackLogger.WriteError($"Error occurred during the {nameof(MsmqProvider)} write operation", ex);
            FallbackLogger.WriteFallback(message);
        }
    }

    /// <summary>
    /// Disposes of the resources used by the <see cref="MsmqProvider"/> instance.
    /// </summary>
    /// <remarks>
    /// This method releases any unmanaged resources and suppresses finalization for the current instance.
    /// </remarks>
    /// <exception cref="Exception">
    /// Thrown when an error occurs during the disposal of the MSMQ queue.
    /// </exception>
    public void Dispose()
    {
        try
        {
            queue.Dispose();
        }
        catch (Exception ex)
        {
            FallbackLogger.WriteError($"Error occurred during {nameof(MsmqProvider)} disposal", ex);
        }
        finally
        {
            GC.SuppressFinalize(this);
        }
    }
}
