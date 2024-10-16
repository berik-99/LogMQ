#if Windows
using MSMQ.Messaging;
using Serilog.Core;
using Serilog.Events;
using Serilog.Sinks.LogMQ.Extensions;
using System.Diagnostics;

namespace Serilog.Sinks.LogMQ;

public class LogMQMSMQSink : ILogEventSink, IDisposable
{
	private readonly IFormatProvider formatProvider;
	private readonly string queuePath = @".\Private$\LogMQ_Queue";
	private readonly string applicationName;
	private readonly MessageQueue queue;

	public LogMQMSMQSink(IFormatProvider formatProvider = null, string queuePath = null, string applicationName = null)
	{
		this.formatProvider = formatProvider;
		this.queuePath = queuePath ?? this.queuePath;
		this.applicationName = string.IsNullOrWhiteSpace(applicationName) ? Process.GetCurrentProcess().ProcessName : applicationName;
		if (!MessageQueue.Exists(this.queuePath))
			MessageQueue.Create(this.queuePath);
		queue = new MessageQueue(this.queuePath);
	}

	public void Emit(LogEvent logEvent)
	{
		var jsonMessage = logEvent.ToJsonLogMessage(formatProvider, applicationName);
		Message message = new()
		{
			Body = jsonMessage,
			Formatter = new ActiveXMessageFormatter(),
			Label = $"LogMQ_{applicationName}_{logEvent.Timestamp.ToString()}"
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