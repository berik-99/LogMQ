using LogMQ.Core;
using LogMQ.Loggers.Serilog.Extensions;
using LogMQ.Providers.Contracts;
using Serilog;
using Serilog.Core;

namespace LogMQ.Loggers.Serilog;

/// <summary>
/// Implements a fallback logger for LogMQ, providing a secondary logging mechanism
/// in case the primary logging system fails.
/// </summary>
/// <remarks>
/// The <see cref="FallbackLogger"/> class ensures continuity in logging by providing
/// a fallback mechanism. If a custom sink is not specified, it defaults to a console-based logger.
/// This fallback logger is useful for ensuring that logging persists even when the main logging provider encounters an issue.
/// </remarks>
/// <param name="fallback">
/// An optional fallback <see cref="ILogEventSink"/>. If not specified, the logger will default
/// to writing log events to the console.
/// </param>
public class FallbackLogger(ILogEventSink fallback = null) : FallbackLogProviderBase
{
	/// <summary>
	/// A lazily-initialized default fallback sink that writes log events to the console.
	/// This is used when no custom fallback sink is provided during the creation of the logger.
	/// </summary>
	/// <remarks>
	/// The default fallback sink is a simple console logger. If no other logging mechanism is specified,
	/// this will be used to ensure logs are captured and written to the console for visibility.
	/// </remarks>
	private static readonly Lazy<ILogEventSink> defaultFallback = new(() => new LoggerConfiguration().WriteTo.Console().CreateLogger());

	/// <summary>
	/// Gets the fallback sink that will be used to emit log events.
	/// If a custom fallback sink was provided, it is used; otherwise, the default console sink is used.
	/// </summary>
	private ILogEventSink Fallback => fallback ?? defaultFallback.Value;

	/// <inheritdoc />
	public override void WriteFallback(LogMessage message) => Fallback.Emit(message.ToLogEvent());

	/// <inheritdoc />
	public override void Write(LogLevel logLevel, string message, Exception ex = null) =>
		(Fallback as Logger).Write(logLevel.ToSerilogLogLevel(), exception: ex, "{Message}", message);
}
