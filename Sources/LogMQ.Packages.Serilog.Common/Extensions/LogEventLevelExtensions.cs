using LogMQ.Contracts;
using Serilog.Events;

namespace LogMQ.Serilog.Extensions;

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

	internal static LogEventLevel ToSerilogLogLevel(this LogLevel logMQLevel) => logMQLevel switch
	{
		LogLevel.Trace => LogEventLevel.Verbose,         // Trace (custom) -> Verbose (Serilog)
		LogLevel.Debug => LogEventLevel.Debug,           // Debug -> Debug
		LogLevel.Information => LogEventLevel.Information, // Information -> Information
		LogLevel.Warning => LogEventLevel.Warning,       // Warning -> Warning
		LogLevel.Error => LogEventLevel.Error,           // Error -> Error
		LogLevel.Critical => LogEventLevel.Fatal,        // Critical (custom) -> Fatal
		_ => LogEventLevel.Information,                 // Default case, e.g., treat None as Information
	};
}
