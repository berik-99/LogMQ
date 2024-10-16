using Serilog.Core;
using Serilog.Events;
using Serilog.Sinks.LogMQ.Extensions;
using System.Diagnostics;

namespace Serilog.Sinks.LogMQ;

public class LogMQBrokerSink(IFormatProvider formatProvider, string host, int port, string applicationName = null) : ILogEventSink
{
	private readonly string applicationName = string.IsNullOrWhiteSpace(applicationName) ? Process.GetCurrentProcess().ProcessName : applicationName;

	public void Emit(LogEvent logEvent)
	{
		try
		{
			var jsonMessage = logEvent.ToJsonLogMessage(null, applicationName);
			Console.WriteLine(jsonMessage);
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Failed to send log message: {ex.Message}");
		}
	}
}