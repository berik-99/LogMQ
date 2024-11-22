using LogMQ.Core;

namespace LogMQ.Providers.Contracts;

/// <summary>
/// Defines the contract for a fallback log provider used to handle errors and log messages
/// when the primary log provider fails.
/// </summary>
public interface IFallbackLogProvider
{
    /// <summary>
    /// Writes a log message to the fallback storage or mechanism.
    /// This method is called when the primary log provider encounters an issue.
    /// </summary>
    /// <param name="message">The log message to be written.</param>
    void WriteFallback(LogMessage message);

    /// <summary>
    /// Writes a log message at the specified <see cref="LogLevel"/>.
    /// </summary>
    /// <param name="logLevel">The severity level of the log message.</param>
    /// <param name="message">The content of the log message.</param>
    /// <param name="ex">An optional exception associated with the log entry.</param>
    void Write(LogLevel logLevel, string message, Exception ex = null);

    /// <summary>
    /// Writes a log message with severity level <see cref="LogLevel.Trace"/>.
    /// </summary>
    /// <param name="message">The content of the log message.</param>
    void WriteTrace(string message);

    /// <summary>
    /// Writes a log message with severity level <see cref="LogLevel.Debug"/>.
    /// </summary>
    /// <param name="message">The content of the log message.</param>
    void WriteDebug(string message);

    /// <summary>
    /// Writes a log message with severity level <see cref="LogLevel.Information"/>.
    /// </summary>
    /// <param name="message">The content of the log message.</param>
    void WriteInformation(string message);

    /// <summary>
    /// Writes a log message with severity level <see cref="LogLevel.Warning"/>.
    /// </summary>
    /// <param name="message">The content of the log message.</param>
    /// <param name="ex">An optional exception associated with the log entry.</param>
    void WriteWarning(string message, Exception ex = null);

    /// <summary>
    /// Writes a log message with severity level <see cref="LogLevel.Error"/>.
    /// </summary>
    /// <param name="message">The content of the log message.</param>
    /// <param name="ex">An optional exception associated with the log entry.</param>
    void WriteError(string message, Exception ex = null);

    /// <summary>
    /// Writes a log message with severity level <see cref="LogLevel.Critical"/>.
    /// </summary>
    /// <param name="message">The content of the log message.</param>
    /// <param name="ex">An optional exception associated with the log entry.</param>
    void WriteCritical(string message, Exception ex = null);
}
