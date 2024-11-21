using LogMQ.Core;
using LogMQ.Loggers.Serilog.Extensions;
using LogMQ.Providers.Contracts;
using Serilog;
using Serilog.Core;

namespace LogMQ.Loggers.Serilog;

public class FallbackLogger(ILogEventSink fallback = null) : IFallbackLogProvider
{
	private static readonly Lazy<ILogEventSink> defaultFallbackSink = new(() => new LoggerConfiguration().WriteTo.Console().CreateLogger());

	public void WriteError(string message, Exception ex = null)
	{
		fallback ??= defaultFallbackSink.Value;
		(fallback as Logger)?.Error(ex, message);
	}

	public void WriteFallback(LogMessage message)
	{
		fallback ??= defaultFallbackSink.Value;
		fallback.Emit(message.ToLogEvent());
	}
}
