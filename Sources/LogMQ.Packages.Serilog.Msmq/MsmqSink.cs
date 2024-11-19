using Msmq.NetCore.Messaging;
using Serilog.Core;
using Serilog.Events;

namespace Serilog.Sinks.LogMQ.Sinks;

internal sealed class MsmqSink : ILogEventSink, IDisposable
{
    private readonly IFormatProvider _formatProvider;
    private readonly string _applicationName;
    private readonly string _category;
    private readonly MessageQueue _queue;
    private readonly ILogEventSink _fallbackLogger;

    internal MsmqSink(IFormatProvider formatProvider, string queuePath, string applicationName, string category, ILogEventSink fallbackLogger)
    {
        try
        {
            _formatProvider = formatProvider;
            _applicationName = applicationName;
            _category = category;
            _fallbackLogger = fallbackLogger;
            if (!MessageQueue.Exists(queuePath)) MessageQueue.Create(queuePath);
            _queue = new MessageQueue(queuePath);
        }
        catch (Exception ex)
        {
            (_fallbackLogger as Logger)?.Error(ex, "An error occurred during LogMQ Sink initialization.");
        }
    }

    public void Emit(LogEvent logEvent)
    {
        try
        {
            var logMsg = logEvent.ToLogMessage(_formatProvider, _applicationName, _category);
            using MemoryStream stream = new();
            logMsg.SerializeToStream(stream);
            Message message = new()
            {
                BodyStream = stream,
                Label = $"LogMQ_{_applicationName}_{logEvent.Timestamp:yyyy-MM-ddTHH:mm:ss.fffZ}"
            };
            _queue.Send(message);
        }
        catch (Exception ex)
        {
            (_fallbackLogger as Logger)?.Error(ex, "An error occurred while writing log to LogMQ MSMQ.");
            _fallbackLogger.Emit(logEvent);
        }
    }

    public void Dispose()
    {
        try
        {
            _queue.Dispose();
            GC.SuppressFinalize(this);
        }
        catch (Exception ex)
        {
            (_fallbackLogger as Logger)?.Error(ex, "An error occurred during LogMQMsmqSink disposal.");
        }
    }
}