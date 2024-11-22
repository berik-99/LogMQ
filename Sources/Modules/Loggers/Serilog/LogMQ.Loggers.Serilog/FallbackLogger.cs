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

    /// <inheritdoc />
    public void WriteFallback(LogMessage message)
    {
        fallback ??= defaultFallbackSink.Value;
        fallback.Emit(message.ToLogEvent());
    }

    /// <inheritdoc />
    public void Write(LogLevel logLevel, string message, Exception ex = null)
    {
        fallback ??= defaultFallbackSink.Value;
        (fallback as Logger).Write(logLevel.ToSerilogLogLevel(), exception: ex, "{Message}", message);
    }

    /// <inheritdoc />
    public void WriteTrace(string message)
    {
        Write(LogLevel.Trace, message);
    }

    /// <inheritdoc />
    public void WriteDebug(string message)
    {
        Write(LogLevel.Debug, message);
    }

    /// <inheritdoc />
    public void WriteInformation(string message)
    {
        Write(LogLevel.Information, message);
    }

    /// <inheritdoc />
    public void WriteWarning(string message, Exception ex = null)
    {
        Write(LogLevel.Warning, message);
    }

    /// <inheritdoc />
    public void WriteError(string message, Exception ex = null)
    {
        Write(LogLevel.Error, message, ex);
    }

    /// <inheritdoc />
    public void WriteCritical(string message, Exception ex = null)
    {
        Write(LogLevel.Critical, message, ex);
    }
}
