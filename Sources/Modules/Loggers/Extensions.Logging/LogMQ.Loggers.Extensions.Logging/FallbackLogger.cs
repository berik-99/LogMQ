using LogMQ.Core;
using LogMQ.Providers.Contracts;
using Microsoft.Extensions.Logging;

namespace LogMQ.Extensions.Logging;

/// <summary>
/// Implements a fallback logger for LogMQ that provides a secondary logging mechanism
/// in case the primary logging system fails, using the Microsoft.Extensions.Logging framework.
/// </summary>
/// <remarks>
/// The <see cref="FallbackLogger"/> class provides a fallback mechanism for logging
/// events when the primary logger is unavailable. By default, it writes logs to the console.
/// If a custom logger is provided, it will be used instead of the default one.
/// This ensures that logging is still operational even if the main logger fails.
/// </remarks>
/// <param name="fallback">
/// An optional fallback <see cref="ILogger"/> instance. If not provided, the logger will
/// default to writing log events to the console.
/// </param>
public sealed class FallbackLogger(ILogger fallback = null) : FallbackLogProviderBase
{
	/// <summary>
	/// A lazily-initialized default fallback logger that writes logs to the console.
	/// This is used when no custom fallback logger is provided during the creation of the logger.
	/// </summary>
	/// <remarks>
	/// The default fallback logger uses <see cref="LoggerFactory"/> to create a logger that writes
	/// log events to the console. If no other logger is specified, this will be the logger used.
	/// </remarks>
	private static readonly Lazy<ILogger> defaultFallback = new(() => LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger(nameof(FallbackLogger)));

	/// <summary>
	/// Gets the fallback logger that will be used to log messages. If a custom logger is provided,
	/// it is used; otherwise, the default console logger is used.
	/// </summary>
	private ILogger Fallback => fallback ?? defaultFallback.Value;

	/// <inheritdoc />
	public override void WriteFallback(LogMessage message)
	{
		Fallback.Log((Microsoft.Extensions.Logging.LogLevel)message.LogLevel, message.Message);
		// TODO: Better management for fallback logs.
		// TODO: Add exception details if present in the log message.
	}

	/// <inheritdoc />
	public override void Write(Core.LogLevel logLevel, string message, Exception ex = null)
	{
		Fallback.Log((Microsoft.Extensions.Logging.LogLevel)logLevel, ex, message);
	}
}
