using LogMQ.Messages;

namespace LogMQ.Providers;

public interface IFallbackLogProvider
{
	public void WriteFallback(LogMessage message);
	public void WriteError(string message, Exception ex = null);
}
