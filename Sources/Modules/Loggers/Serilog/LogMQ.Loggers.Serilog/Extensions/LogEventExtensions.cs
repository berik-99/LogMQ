using LogMQ.Core;
using Serilog;
using Serilog.Events;
using Serilog.Parsing;
using System.Diagnostics;

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
            Application = new LogApplication()
            {
                Name = applicationName,
                Category = category,
                Machine = Environment.MachineName,
                Pid = Environment.ProcessId,
            },
            Meta = logEvent.GetMetadata()
        };
    }

    /// <summary>
    /// Extracts metadata from the current log event properties, and if not found, generates the metadata
    /// automatically from the stack trace and an optional log event exception.
    /// </summary>
    /// <param name="logEvent">The log event containing information about the current log entry, including an optional exception.</param>
    /// <returns>
    /// A <see cref="LogMetadata"/> object populated with details about the class, method, method signature,
    /// file, line number, and exception (if present). If the properties are not found in the log event, the metadata
    /// is generated automatically based on the current stack trace and any exception present.
    /// </returns>
    /// <remarks>
    /// The method first attempts to extract metadata from the <see cref="LogEvent"/> properties.
    /// If any required properties are missing, it generates them using the current stack trace. The stack trace analysis
    /// skips frames related to the logging assembly (<see cref="Log"/>). It also handles asynchronous methods,
    /// lambda functions, and their signatures.
    /// <note type="note">
    /// Automatic metadata generation works only within enrichers and sinks that are called synchronously.
    /// </note>
    /// </remarks>
    internal static LogMetadata GetMetadata(this LogEvent logEvent)
    {
        if (logEvent.TryGetLogMetadata(out LogMetadata logMetadata))
            return logMetadata;

        var frame = EnhancedStackTrace.Current().Take(128).Skip(3)
                          .FirstOrDefault(f => f.HasMethod() && f.MethodInfo.DeclaringType?.Assembly != typeof(Log).Assembly);

        if (frame == null) return logMetadata;

        var methodInfo = frame.MethodInfo;
        logMetadata.Class = methodInfo.DeclaringType?.FullName;
        logMetadata.MethodName = methodInfo.Name;
        logMetadata.MethodSignature = methodInfo.IsLambda ? "Lambda [" : "";
        logMetadata.MethodSignature += $"{(methodInfo.IsAsync ? "Async" : "")} {methodInfo.ReturnParameter?.ResolvedType?.FullName ?? "void"} {methodInfo.Name}";
        var parameters = methodInfo.Parameters;
        logMetadata.MethodSignature += parameters.Count > 0
            ? $"({string.Join(", ", parameters.Select(p => $"{p.ResolvedType.FullName} {p.Name}"))})"
            : "()";
        logMetadata.MethodSignature += methodInfo.IsLambda ? "]" : "";
        logMetadata.File = frame.GetFileName();
        logMetadata.Row = frame.GetFileLineNumber();
        logMetadata.Exception = (logEvent.Exception?.ToString());

        return logMetadata;
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
    /// with exception details and custom properties derived from the <see cref="LogMessage"/>.
    /// </remarks>
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

    /// <summary>
    /// Tries to extract a <see cref="LogMetadata"/> object from the provided <see cref="LogEvent"/> properties.
    /// If any property is missing or cannot be converted, the method returns <c>false</c>.
    /// </summary>
    /// <param name="logEvent">The <see cref="LogEvent"/> from which the metadata should be extracted.</param>
    /// <param name="metadata">The extracted <see cref="LogMetadata"/> if successful, otherwise the default value.</param>
    /// <returns><c>true</c> if all properties are successfully extracted and converted, otherwise <c>false</c>.</returns>
    private static bool TryGetLogMetadata(this LogEvent logEvent, out LogMetadata metadata)
    {
        metadata = new LogMetadata();

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

        string className = null, methodName = null, methodSignature = null, exception = null;
        int row = 0;
        bool allPropertiesExtracted = TryGetProperty(nameof(LogMetadata.File), out string file) &&
                                      TryGetProperty(nameof(LogMetadata.Class), out className) &&
                                      TryGetProperty(nameof(LogMetadata.MethodName), out methodName) &&
                                      TryGetProperty(nameof(LogMetadata.MethodSignature), out methodSignature) &&
                                      TryGetProperty(nameof(LogMetadata.Row), out row) &&
                                      TryGetProperty(nameof(LogMetadata.Exception), out exception);

        if (allPropertiesExtracted)
        {
            metadata.File = file;
            metadata.Class = className;
            metadata.MethodName = methodName;
            metadata.MethodSignature = methodSignature;
            metadata.Row = row;
            metadata.Exception = exception;
        }

        return allPropertiesExtracted;
    }
}
