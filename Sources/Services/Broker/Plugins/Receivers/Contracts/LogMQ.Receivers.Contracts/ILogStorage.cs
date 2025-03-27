using LogMQ.Core;

namespace LogMQ.Receivers.Contracts;

public interface ILogStorage
{
    Task WriteLogMessageAsync(LogMessage logMessage);
}