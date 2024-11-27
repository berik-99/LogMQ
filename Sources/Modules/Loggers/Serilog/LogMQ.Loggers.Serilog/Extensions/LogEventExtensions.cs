using LogMQ.Core;
using Serilog;
using Serilog.Events;
using Serilog.Parsing;

namespace LogMQ.Loggers.Serilog.Extensions;

/// <summary>
/// Provides extension methods to convert between Serilog's <see cref="LogEvent"/> and LogMQ's <see cref="LogMessage"/>.
/// </summary>
/// <remarks>
/// This class facilitates the integration of Serilog with LogMQ by enabling conversions and metadata extraction
/// necessary for logging compatibility.
/// </remarks>
internal static class LogEventExtensions
{
	/// <summary>
	/// Converts a Serilog <see cref="LogEvent"/> into a LogMQ <see cref="LogMessage"/>.
	/// </summary>
	/// <param name="logEvent">The Serilog log event to convert.</param>
	/// <param name="formatProvider">
	/// An <see cref="IFormatProvider"/> used to format the message string.
	/// </param>
	/// <param name="applicationName">The name of the application generating the log.</param>
	/// <param name="category">The category of the application.</param>
	/// <returns>
	/// A <see cref="LogMessage"/> instance containing structured log details compatible with LogMQ.
	/// </returns>
	/// <remarks>
	/// This method extracts log event properties, including timestamp, log level, message content,
	/// and application details, while also retrieving metadata from the log event's stack trace.
	/// </remarks>
	internal static LogMessage ToLogMessage(this LogEvent logEvent, IFormatProvider formatProvider, string applicationName, string category)
	{
		return new()
		{
			Guid = Guid.NewGuid(),
			Timestamp = logEvent.Timestamp,
			LogLevel = logEvent.Level.ToLogMQLogLevel(),
			Message = logEvent.RenderMessage(formatProvider),
			ExceptionMessage = logEvent.Exception?.Message,
			Application = new LogApplication()
			{
				Name = applicationName,
				Category = category,
				Machine = Environment.MachineName,
				Pid = Environment.ProcessId,
			},
			Metadata = logEvent.ExtractLogMetadata()
		};
	}

	/// <summary>
	/// Converts a LogMQ <see cref="LogMessage"/> into a Serilog <see cref="LogEvent"/>.
	/// </summary>
	/// <param name="logMessage">The LogMQ log message to convert.</param>
	/// <returns>
	/// A <see cref="LogEvent"/> instance that can be processed by Serilog sinks.
	/// </returns>
	/// <remarks>
	/// This method maps LogMQ's log levels to Serilog's, reconstructs the message template, and creates a log event
	/// with exMsg details and custom properties derived from the <see cref="LogMessage"/>.
	/// </remarks>
	internal static LogEvent ToLogEvent(this LogMessage logMessage)
	{
		var serilogLevel = logMessage.LogLevel.ToSerilogLogLevel();
		var messageTemplate = new MessageTemplateParser().Parse(logMessage.Message ?? string.Empty);
		var exMsg = logMessage.ExceptionMessage;
		var properties = new List<LogEventProperty>
		{
			new(nameof(LogApplication.Name), new ScalarValue(logMessage.Application.Name)),
			new(nameof(LogApplication.Category), new ScalarValue(logMessage.Application.Category)),
			new(nameof(LogApplication.Machine), new ScalarValue(logMessage.Application.Machine)),
			new(nameof(LogApplication.Pid), new ScalarValue(logMessage.Application.Pid)),
			new(nameof(LogMetadata), new ScalarValue(logMessage.Metadata))
		};
		return new LogEvent(
			logMessage.Timestamp,
			serilogLevel,
			new Exception(exMsg),
			messageTemplate,
			properties
		);
	}

	/// <summary>
	/// Extracts metadata for the current log event, prioritizing pre-enriched properties.
	/// </summary>
	/// <param name="logEvent">
	/// The log event containing information about the current log entry, including optional pre-enriched metadata 
	/// (e.g., added by <see cref="LogMQEnricher"/>) or an exception message.
	/// </param>
	/// <returns>
	/// A <see cref="LogMetadata"/> object populated with details such as class name, method name, method signature, 
	/// file path, line number, and an optional exception message (exMsg).  
	/// If these properties are not found in the log event, metadata is generated dynamically using 
	/// the <see cref="LogMetadata.GetLogMetadata"/> method, which analyzes the current stack trace.
	/// </returns>
	/// <remarks>
	/// This method operates in two phases:
	/// <list type="number">
	/// <item>
	/// It first attempts to extract metadata directly from the <see cref="LogEvent"/> properties.  
	/// This supports scenarios where a `LogMQEnricher` or similar has pre-populated the metadata.
	/// </item>
	/// <item>
	/// If the metadata properties are missing or incomplete, the method falls back to analyzing the stack trace 
	/// via <see cref="LogMetadata.GetLogMetadata"/>. This ensures that logging is robust even in environments 
	/// without enrichers or when enrichers fail to provide sufficient information.
	/// </item>
	/// </list>
	/// <para><b>Stack Trace Analysis:</b></para>
	/// When using <see cref="LogMetadata.GetLogMetadata"/> for dynamic metadata generation:
	/// <list type="bullet">
	/// <item>Frames related to the logging infrastructure (e.g., <see cref="Log"/> class) are skipped.</item>
	/// <item>Common .NET constructs like asynchronous methods and lambdas are supported.</item>
	/// </list>
	/// <note type="note">
	/// The accuracy of stack trace-based metadata generation depends on the method being executed synchronously.  
	/// Asynchronous logging scenarios may result in incomplete or less precise metadata.
	/// </note>
	/// </remarks>
	internal static LogMetadata ExtractLogMetadata(this LogEvent logEvent)
	{
		// Helper method to extract a property of a given type from the LogEvent.
		bool TryGetProperty<T>(string propertyName, out T value)
		{
			value = default;
			if (logEvent.Properties.TryGetValue(propertyName, out var propertyValue) &&
				propertyValue is ScalarValue scalarValue)
			{
				value = (T)Convert.ChangeType(scalarValue.Value, typeof(T));
				return true;
			}
			return false;
		}

		string className = null, methodName = null, methodSignature = null;
		int row = 0;
		bool allPropertiesExtracted = TryGetProperty(nameof(LogMetadata.File), out string file) &&
									  TryGetProperty(nameof(LogMetadata.Class), out className) &&
									  TryGetProperty(nameof(LogMetadata.MethodName), out methodName) &&
									  TryGetProperty(nameof(LogMetadata.MethodSignature), out methodSignature) &&
									  TryGetProperty(nameof(LogMetadata.Row), out row);
		return allPropertiesExtracted
			? new() { File = file, Class = className, MethodName = methodName, MethodSignature = methodSignature, Row = row }
			: LogMetadata.GetLogMetadata(typeof(Log));
	}
}
