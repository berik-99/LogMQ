using LogMQ.Core;
using LogMQ.Providers.Contracts;
using System.Collections.Concurrent;
using System.Text;
using WatsonTcp;

namespace LogMQ.Providers;

/// <summary>
/// A Socket Tcp log provider for sending log messages to the Tcp Receiver inside LogMQ Broker.
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

    /// <summary>
    /// Default maximum number of slots in the retry queue for storing log messages that failed to send.
    /// </summary>
    public const int DefaultRetryQueueSize = 50;

    /// <summary>
    /// The TCP client used for communicating with the LogMQ Broker.
    /// Responsible for establishing and maintaining the TCP connection.
    /// </summary>
    private readonly WatsonTcpClient tcpClient;

    /// <summary>
    /// The retry queue for storing log messages that failed to send, used for retrying the send operation.
    /// </summary>
    private readonly ConcurrentQueue<LogMessage> retryQueue;

    /// <summary>
    /// The maximum length of the retry queue.
    /// </summary>
    private readonly int retryQueueLength;

    /// <inheritdoc />
    public IFormatProvider FormatProvider { get; }

    /// <inheritdoc />
    public IFallbackLogProvider FallbackLogger { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="TcpProvider"/> class.
    /// Establishes a connection to the LogMQ broker's TCP module at the specified address.
    /// </summary>
    /// <param name="formatProvider">
    /// The format provider for log message formatting.
    /// </param>
    /// <param name="host">
    /// The TCP host address of the LogMQ Broker. Cannot be null or empty.
    /// </param>
    /// <param name="port">
    /// The TCP port of the LogMQ Broker.
    /// </param>
    /// <param name="fallbackLogger">
    /// The fallback logger for error handling and fallback logging. Cannot be null.
    /// </param>
    /// <param name="retryQueueSize">
    /// The maximum number of log messages that can be held in the retry queue for resending in case of failure.
    /// Defaults to <see cref="DefaultRetryQueueSize"/>. If set to 0, the retry queue is disabled,
    /// and any error during log transmission will immediately trigger fallback logging.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when any of <paramref name="formatProvider"/>, <paramref name="host"/>, or <paramref name="fallbackLogger"/> is null or empty.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="retryQueueSize"/> is negative.
    /// </exception>
    public TcpProvider(IFormatProvider formatProvider, string host, int port, IFallbackLogProvider fallbackLogger, int retryQueueSize = DefaultRetryQueueSize)
    {
        ArgumentNullException.ThrowIfNull(fallbackLogger);
        ArgumentNullException.ThrowIfNullOrWhiteSpace(host);
        ArgumentOutOfRangeException.ThrowIfNegative(retryQueueSize);
        FallbackLogger = fallbackLogger;
        FormatProvider = formatProvider;
        retryQueue = new ConcurrentQueue<LogMessage>();
        retryQueueLength = retryQueueSize;
        tcpClient = new WatsonTcpClient(host, port);
        tcpClient.Events.ServerDisconnected += (s, e) => TryReconnect();
        tcpClient.Events.MessageReceived += (s, e) => { };
        tcpClient.Connect();
        Task.Run(Ping).Wait();
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
    /// Attempts to reconnect to the LogMQ Broker if the connection is lost.
    /// Retries indefinitely unless a retry limit is set elsewhere.
    /// </summary>
    private void TryReconnect()
    {
        int retryCount = 0;
        while (!tcpClient.Connected)
        {
            try
            {
                tcpClient.Connect();
            }
            catch (Exception ex)
            {
                FallbackLogger.WriteError($"Error occurred during the reconnection operation. Attempt #{retryCount}", ex);
                Thread.Sleep(1000);
                retryCount++;
            }
        }
    }

    /// <summary>
    /// Asynchronously sends a log message to the LogMQ Broker.
    /// If the send operation fails, the behavior depends on the value of <paramref name="enqueueErrors"/>:
    /// - If true, the failed message is added to the retry queue.
    /// - If false, the failure is logged, but the message is not enqueued for retry.
    /// </summary>
    /// <param name="message">
    /// The log message to be sent to the LogMQ Broker. Cannot be null.
    /// </param>
    /// <param name="enqueueErrors">
    /// A flag indicating whether to enqueue the message in case of a send failure.
    /// If set to true, failed messages are added to the retry queue (if space is available).
    /// If set to false, failed messages are logged to the fallback logger without retrying.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous send operation.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the log message fails to send to the LogMQ Broker and <paramref name="enqueueErrors"/> is false.
    /// </exception>
    /// <remarks>
    /// If <paramref name="enqueueErrors"/> is true and the retry queue is full, the oldest message in the queue
    /// is discarded to make room for the new message. In both cases, failures are forwarded to the fallback logger.
    /// </remarks>
    private async Task WriteAsync(LogMessage message, bool enqueueErrors)
    {
        try
        {
            var bin = message.Serialize();
            if (!await tcpClient.SendAsync(bin))
                throw new InvalidOperationException("Failed to send log message to LogMQ Broker");
        }
        catch (Exception ex) when (enqueueErrors)
        {
            if (retryQueue.Count >= retryQueueLength)
            {
                retryQueue.TryDequeue(out _);
                FallbackLogger.WriteError("Error occurred while writing log to LogMQ Broker. Retry queue is full", ex);
                FallbackLogger.WriteFallback(message);
            }
            else
            {
                FallbackLogger.WriteWarning($"Error occurred while writing log to LogMQ Broker. Retry queue is {retryQueue.Count}/{retryQueueLength}", ex);
                FallbackLogger.WriteFallback(message);
            }
            retryQueue.Enqueue(message);
        }
    }

    /// <summary>
    /// Asynchronously processes and sends all log messages in the retry queue to the LogMQ Broker.
    /// This method attempts to send each message from the retry queue, one at a time.
    /// If an error occurs during the send operation, the message will remain in the retry queue for a future attempt.
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation of sending messages from the retry queue.
    /// </returns>
    /// <remarks>
    /// If any errors occur while sending messages from the retry queue, the error is logged with either a warning or an error level,
    /// depending on the current size of the retry queue.
    /// </remarks>
    private async Task WriteFromRetryQueueAsync()
    {
        try
        {
            while (retryQueue.TryPeek(out LogMessage queueMsg))
            {
                await WriteAsync(queueMsg, false);
                retryQueue.TryDequeue(out _);
            }
        }
        catch (Exception ex)
        {
            FallbackLogger.Write(
                retryQueue.Count >= retryQueueLength ? LogLevel.Error : LogLevel.Warning,
                $"Error occurred while writing logs from retry queue to LogMQ Broker. Retry queue is {retryQueue.Count}/{retryQueueLength}",
                ex);
        }
    }

    /// <inheritdoc />
    public void Write(LogMessage message)
    {
        try
        {
            WriteFromRetryQueueAsync().Wait();
            WriteAsync(message, true).Wait();
        }
        catch (Exception ex)
        {
            Exception baseException = ex is AggregateException aex ? aex.GetBaseException() : ex;
            FallbackLogger.WriteError("Error occurred during the Write operation", baseException);
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
            retryQueue.Clear();
        }
        catch (Exception ex)
        {
            FallbackLogger.WriteError($"Error occurred during {nameof(TcpProvider)} disposal", ex);
        }
        finally
        {
            GC.SuppressFinalize(this);
        }
    }
}
