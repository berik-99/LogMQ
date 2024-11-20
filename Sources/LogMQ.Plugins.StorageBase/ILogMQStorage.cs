using LogMQ.Messages;

namespace LogMQ.Plugins.Storage.Contracts;

public interface ILogMQStorage
{
	public abstract Task WriteLogMessage(LogMessage logMessage);
}
