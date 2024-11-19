using LogMQ.Providers.Broker;
using Serilog.Core;
using Serilog.Events;

namespace Serilog.Sinks.LogMQ.Sinks;

internal sealed class TcpSink(IFormatProvider formatProvider, string host, int port, string applicationName, string category, TcpProvider fallbackLogger) 
	: TcpProvider(formatProvider, host, port, applicationName, category, fallbackLogger), ILogEventSink
{
	public void Emit(LogEvent logEvent)
	{
		try
		{
			var logMsg = logEvent.ToLogMessage(formatProvider, applicationName, category);
			Send(logMsg).Wait();
		}
		catch (Exception ex)
		{
			//(fallbackLogger as Logger)?.Error(ex, "Error occurred while writing log to LogMQ Broker");
			//fallbackLogger.Emit(logEvent);
		}
	}
}
