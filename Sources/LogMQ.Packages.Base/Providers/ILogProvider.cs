using LogMQ.Messages;

namespace LogMQ.Providers;

public interface ILogProvider
{
	public IFormatProvider FormatProvider { get; }
	public IFallbackLogProvider FallbackLogger { get; }

	public void Write(LogMessage message);
}
