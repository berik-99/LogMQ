using LogMQ.Core;
using LogMQ.Loggers.Serilog.Extensions;
using LogMQ.Providers.Contracts;
using Serilog;
using Serilog.Core;

namespace LogMQ.Loggers.Serilog;

/// <summary>
/// A fallback logger implementation for LogMQ that ensures logging continuity in case of primary logging failures.
/// </summary>
/// <remarks>
/// The <see cref="FallbackLogger"/> provides a fallback mechanism for logging operations.
/// If a custom sink is not provided, it defaults to a console-based logger.
/// </remarks>
/// <param name="fallback">
/// An optional fallback <see cref="ILogEventSink"/>. If not specified, a console-based sink is used by default.
/// </param>
public class FallbackLogger(ILogEventSink fallback = null) : IFallbackLogProvider
{
    /// <summary>
    /// A lazily-initialized default fallback sink that writes logs to the console.
    /// </summary>
    /// <remarks>
    /// This sink is used when no custom fallback sink is provided during construction.
    /// </remarks>
    private static readonly Lazy<ILogEventSink> defaultFallbackSink = new(() => new LoggerConfiguration().WriteTo.Console().CreateLogger());

    /// <summary>
    /// Writes an error message to the fallback logger, optionally including exception details.
    /// </summary>
    /// <param name="message">The error message to log.</param>
    /// <param name="ex">The exception to include in the log entry, if any. Default is <c>null</c>.</param>
    /// <remarks>
    /// If the <see cref="fallback"/> logger is not set, the method initializes it to the default console-based sink.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="message"/> is <c>null</c>.</exception>
    public void WriteError(string message, Exception ex = null)
    {
        fallback ??= defaultFallbackSink.Value;
        (fallback as Logger)?.Error(ex, "{Message}", message);
    }

    /// <summary>
    /// Writes a log message to the fallback logger.
    /// </summary>
    /// <param name="message">The <see cref="LogMessage"/> to log.</param>
    /// <remarks>
    /// The log message is converted to a <see cref="LogEvent"/> before being passed to the fallback logger.
    /// If the <see cref="fallback"/> logger is not set, the method initializes it to the default console-based sink.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="message"/> is <c>null</c>.</exception>
    public void WriteFallback(LogMessage message)
    {
        fallback ??= defaultFallbackSink.Value;
        fallback.Emit(message.ToLogEvent());
    }
}
