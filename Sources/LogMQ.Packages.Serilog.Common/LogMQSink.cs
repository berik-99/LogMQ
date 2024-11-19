using LogMQ.Providers;
using LogMQ.Serilog.Extensions;
using Serilog.Core;
using Serilog.Events;

namespace LogMQ.Serilog;

internal sealed class LogMQSink(ILogProvider provider, string applicationName, string category) : ILogEventSink
{
	public void Emit(LogEvent logEvent)
	{
		provider.Write(logEvent.ToLogMessage(provider.FormatProvider, applicationName, category));
	}
}
