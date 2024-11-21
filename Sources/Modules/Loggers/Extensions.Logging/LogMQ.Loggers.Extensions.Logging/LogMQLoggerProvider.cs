using LogMQ.Providers.Contracts;
using Microsoft.Extensions.Logging;

namespace LogMQ.Extensions.Logging;

internal sealed class LogMQLoggerProvider(ILogProvider provider, string applicationName) : ILoggerProvider
{
	public ILogger CreateLogger(string categoryName) => new LogMQLogger(provider, applicationName, categoryName);

	public void Dispose() { }
}
