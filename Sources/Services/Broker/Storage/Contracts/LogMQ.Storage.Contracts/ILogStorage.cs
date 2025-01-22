using LogMQ.Core;

namespace LogMQ.Storage.Contracts;

//TODO: Add documentation
public interface ILogStorage
{
    Task WriteLogMessageAsync(LogMessage logMessage);

    Task<List<LogMessage>> GetLogsAsync(LogFilter filter);

    Task<Wrapper<long>> GetTotalLogsCountAsync(Wrapper<string> applicationName);

    Task<List<string>> GetLogApplications();
}