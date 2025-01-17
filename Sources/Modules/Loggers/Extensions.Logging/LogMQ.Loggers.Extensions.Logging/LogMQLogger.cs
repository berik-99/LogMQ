using LogMQ.Core;
using LogMQ.Providers.Contracts;
using Microsoft.Extensions.Logging;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace LogMQ.Extensions.Logging;

/// <summary>
/// An implementation of <see cref="ILogger"/> to integrate LogMQ logging with the .NET logging framework.
/// </summary>
/// <param name="provider">The LogMQ provider responsible for writing log messages.</param>
/// <param name="applicationName">The name of the application generating the logs.</param>
/// <param name="category">The logger category, typically associated with the context of the code writing the logs.</param>
internal sealed class LogMQLogger(ILogProvider provider, string applicationName, string category) : ILogger
{
	/// <summary>
	/// Begins a logging scope. This implementation does not support scoped logging.
	/// </summary>
	/// <typeparam name="TState">The type of the scope state.</typeparam>
	/// <param name="state">The state of the scope.</param>
	/// <returns>Always returns <c>null</c>.</returns>
	public IDisposable BeginScope<TState>(TState state) => null;

	/// <summary>
	/// Checks if the specified log level is enabled.
	/// </summary>
	/// <param name="logLevel">The log level to check.</param>
	/// <returns><c>true</c> if the log level is not <see cref="LogLevel.None"/>; otherwise, <c>false</c>.</returns>
	public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

	/// <summary>
	/// Logs a message with the specified log level, event ID, and state.
	/// </summary>
	/// <typeparam name="TState">The type of the state object associated with the log message.</typeparam>
	/// <param name="logLevel">The severity level of the log message.</param>
	/// <param name="eventId">An identifier for the event being logged.</param>
	/// <param name="state">The state associated with the log message.</param>
	/// <param name="exception">An exception related to the log message, or <c>null</c> if none.</param>
	/// <param name="formatter">A function to format the log message.</param>
	public void Log<TState>(
		LogLevel logLevel,
		EventId eventId,
		TState state,
		Exception exception,
		Func<TState, Exception, string> formatter)
	{
		if (!IsEnabled(logLevel))
			return;

        string message = formatter?.Invoke(state, exception);
        LogMessage logMessage = new()
        {
			Guid = Guid.NewGuid(),
			Timestamp = DateTime.UtcNow,
			Message = message,
			LogLevel = (Core.LogLevel)logLevel.GetHashCode(),
			Metadata = LogMetadata.GetLogMetadata(typeof(ILogger)),
			Application = new()
			{
				Category = category,
				Name = applicationName,
				Machine = Environment.MachineName,
				Pid = Environment.ProcessId,
			}
		};
		provider.Write(logMessage);
	}
}
