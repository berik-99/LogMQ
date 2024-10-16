using LogMQ;
using Serilog.Events;

namespace Serilog.Sinks.LogMQ.Extensions;

internal static class LogEventLevelExtensions
{
	internal static LogLevel ToLogMQLogLevel(this LogEventLevel serilogLevel) => serilogLevel switch
	{
		LogEventLevel.Verbose => LogLevel.Trace,         // Verbose (Serilog) -> Trace (custom)
		LogEventLevel.Debug => LogLevel.Debug,           // Debug -> Debug
		LogEventLevel.Information => LogLevel.Information, // Information -> Information
		LogEventLevel.Warning => LogLevel.Warning,       // Warning -> Warning
		LogEventLevel.Error => LogLevel.Error,           // Error -> Error
		LogEventLevel.Fatal => LogLevel.Critical,        // Fatal -> Critical (custom equivalent)
		_ => LogLevel.None,                              // Default case, if needed
	};
}
