#if Windows
using MSMQ.Messaging;
using Serilog.Core;
using Serilog.Events;
using Serilog.Sinks.LogMQ.Extensions;
using System.Diagnostics;

namespace Serilog.Sinks.LogMQ;

public class LogMQMSMQSink : ILogEventSink, IDisposable
{
    private readonly IFormatProvider _formatProvider;
    private readonly string _queuePath;
    private readonly string _applicationName;
    private readonly MessageQueue queue;

    public LogMQMSMQSink(IFormatProvider formatProvider = null, string queuePath = null, string applicationName = null)
    {
        _formatProvider = formatProvider;
        _queuePath = queuePath ?? _queuePath;
        _applicationName = string.IsNullOrWhiteSpace(applicationName) ? Process.GetCurrentProcess().ProcessName : applicationName;
        if (!MessageQueue.Exists(_queuePath))
            MessageQueue.Create(_queuePath);
        queue = new MessageQueue(_queuePath);
    }

    public void Emit(LogEvent logEvent)
    {
        var logMsg = logEvent.ToLogMessage(_formatProvider, _applicationName);

        using MemoryStream stream = new();
        logMsg.SerializeToStream(stream);
        Message message = new()
        {
            BodyStream = stream,
            Label = $"LogMQ_{_applicationName}_{logEvent.Timestamp}"
        };
        queue.Send(message);
    }

    public void Dispose()
    {
        queue.Dispose();
        GC.SuppressFinalize(this);
    }
}
#endif