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
    private readonly string _applicationName;
    private readonly MessageQueue queue;

    public LogMQMSMQSink(IFormatProvider formatProvider = null, string queuePath = null, string applicationName = null)
    {
        //TODO: handle exceptions
        _formatProvider = formatProvider;
        _applicationName = string.IsNullOrWhiteSpace(applicationName) ? Process.GetCurrentProcess().ProcessName : applicationName;
        if (!MessageQueue.Exists(queuePath))
            MessageQueue.Create(queuePath);
        queue = new MessageQueue(queuePath);
    }

    public void Emit(LogEvent logEvent)
    {
        //TODO: handle exceptions
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