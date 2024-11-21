using LogMQ.Loggers.Serilog.Extensions;
using LogMQ.Providers.Contracts;
using Serilog.Core;
using Serilog.Events;

namespace LogMQ.Loggers.Serilog;

internal sealed class LogMQSink(ILogProvider provider, string applicationName, string category) : ILogEventSink
{
	public void Emit(LogEvent logEvent)
	{
		provider.Write(logEvent.ToLogMessage(provider.FormatProvider, applicationName, category));
	}
}
