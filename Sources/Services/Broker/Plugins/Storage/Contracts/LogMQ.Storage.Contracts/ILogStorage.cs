using LogMQ.Core;
using LogMQ.Services.Shared.LogManager;

namespace LogMQ.Storage.Contracts;

/// <summary>
/// Defines a contract for a storage mechanism that handles LogMQ messages.
/// </summary>
public interface ILogStorage
{
    /// <summary>
    /// Writes a log message to the storage system.
    /// </summary>
    /// <param name="logMessage">The <see cref="LogMessage"/> to be stored.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task WriteLogMessageAsync(LogMessage logMessage);

    Task<List<LogMessage>> GetLogsByFilterAsync(LogFilter filter);

	Task<List<LogMessage>> GetLastLogsAsync(LastLogsFilter filter);
}
