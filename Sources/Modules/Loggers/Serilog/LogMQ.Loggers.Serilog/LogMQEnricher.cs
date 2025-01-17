using LogMQ.Core;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace LogMQ.Loggers.Serilog;

/// <summary>
/// The <c>LogMQEnricher</c> class enriches log messages with structured metadata
/// specific to LogMQ providers. It ensures that the log events carry additional
/// contextual information required by LogMQ.
/// </summary>
internal sealed class LogMQEnricher : ILogEventEnricher
{
	/// <summary>
	/// Enriches the provided <see cref="LogEvent"/> with LogMQ-specific metadata properties.
	/// </summary>
	/// <param name="logEvent">
	/// The <see cref="LogEvent"/> instance to be enriched with metadata.
	/// </param>
	/// <param name="propertyFactory">
	/// A factory for creating structured properties to attach to the log event.
	/// </param>
	public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
	{
        LogMetadata meta = LogMetadata.GetLogMetadata(typeof(Log));

        LogEventProperty[] properties = new[]
		{
			propertyFactory.CreateProperty(nameof(meta.File), meta.File),
			propertyFactory.CreateProperty(nameof(meta.Class), meta.Class),
			propertyFactory.CreateProperty(nameof(meta.MethodName), meta.MethodName),
			propertyFactory.CreateProperty(nameof(meta.MethodSignature), meta.MethodSignature),
			propertyFactory.CreateProperty(nameof(meta.Row), meta.Row)
		};

		foreach (LogEventProperty property in properties)
		{
			logEvent.AddPropertyIfAbsent(property);
		}
	}
}
