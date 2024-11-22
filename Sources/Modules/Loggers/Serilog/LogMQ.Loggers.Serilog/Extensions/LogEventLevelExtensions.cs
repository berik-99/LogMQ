using LogMQ.Core;
using Serilog.Events;

namespace LogMQ.Loggers.Serilog.Extensions;

/// <summary>
/// Provides extension methods to convert between Serilog's <see cref="LogEventLevel"/>
/// and LogMQ's <see cref="LogLevel"/>.
/// </summary>
/// <remarks>
/// This class bridges the gap between Serilog's logging levels and the custom levels defined in LogMQ,
/// ensuring compatibility when using both systems.
/// </remarks>
internal static class LogEventLevelExtensions
{
    /// <summary>
    /// Converts a Serilog <see cref="LogEventLevel"/> to the equivalent LogMQ <see cref="LogLevel"/>.
    /// </summary>
    /// <param name="serilogLevel">The Serilog log level to convert.</param>
    /// <returns>
    /// The corresponding <see cref="LogLevel"/> in the LogMQ system. Defaults to <see cref="LogLevel.None"/>
    /// if the Serilog level is not recognized.
    /// </returns>
    /// <remarks>
    /// This mapping ensures that Serilog's log levels align with the custom levels defined in LogMQ.
    /// For example, <see cref="LogEventLevel.Verbose"/> maps to <see cref="LogLevel.Trace"/>.
    /// </remarks>
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

    /// <summary>
    /// Converts a LogMQ <see cref="LogLevel"/> to the equivalent Serilog <see cref="LogEventLevel"/>.
    /// </summary>
    /// <param name="logMQLevel">The LogMQ log level to convert.</param>
    /// <returns>
    /// The corresponding <see cref="LogEventLevel"/> in the Serilog system. Defaults to <see cref="LogEventLevel.Information"/>
    /// if the LogMQ level is not recognized.
    /// </returns>
    /// <remarks>
    /// This mapping ensures compatibility when converting custom LogMQ levels to Serilog's predefined levels.
    /// For example, <see cref="LogLevel.Trace"/> maps to <see cref="LogEventLevel.Verbose"/>.
    /// </remarks>
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
