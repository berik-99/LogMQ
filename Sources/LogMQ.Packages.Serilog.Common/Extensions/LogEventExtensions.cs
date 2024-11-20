using LogMQ.Contracts;
using Serilog.Events;
using Serilog.Parsing;

namespace LogMQ.Serilog.Extensions;

internal static class LogEventExtensions
{
	internal static LogMessage ToLogMessage(this LogEvent logEvent, IFormatProvider formatProvider, string applicationName, string category) => new()
	{
		Timestamp = logEvent.Timestamp,
		LogLevel = logEvent.Level.ToLogMQLogLevel(),
		Message = logEvent.RenderMessage(formatProvider),
		Application = new LogApplication()
		{
			Name = applicationName,
			Category = category,
			Machine = Environment.MachineName,
			Pid = Environment.ProcessId,
		},
		Meta = LogMetadata.GetMetadata(logEvent.Exception)
	};

	internal static LogEvent ToLogEvent(this LogMessage logMessage)
	{
		var serilogLevel = logMessage.LogLevel.ToSerilogLogLevel();
		var messageTemplate = new MessageTemplateParser().Parse(logMessage.Message ?? string.Empty);
		var exception = new Exception(logMessage.Meta?.Exception);
		var properties = new List<LogEventProperty>
		{
			new(nameof(LogApplication.Name), new ScalarValue(logMessage.Application.Name)),
			new(nameof(LogApplication.Category), new ScalarValue(logMessage.Application.Category)),
			new(nameof(LogApplication.Machine), new ScalarValue(logMessage.Application.Machine)),
			new(nameof(LogApplication.Pid), new ScalarValue(logMessage.Application.Pid)),
			new(nameof(LogMetadata), new ScalarValue(logMessage.Meta))
		};
		return new LogEvent(
			logMessage.Timestamp,
			serilogLevel,
			exception,
			messageTemplate,
			properties
		);
	}
}
