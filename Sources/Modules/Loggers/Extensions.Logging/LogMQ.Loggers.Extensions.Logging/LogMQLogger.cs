using LogMQ.Core;
using LogMQ.Providers.Contracts;
using Microsoft.Extensions.Logging;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace LogMQ.Extensions.Logging;

internal sealed class LogMQLogger(ILogProvider provider, string applicationName, string category) : ILogger
{
    public IDisposable BeginScope<TState>(TState state) => null;

    public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception exception,
        Func<TState, Exception, string> formatter)
    {
        if (!IsEnabled(logLevel))
            return;

        var message = formatter?.Invoke(state, exception);
        var logMessage = new LogMessage
        {
            Guid = Guid.NewGuid(),
            Timestamp = DateTime.UtcNow,
            Message = message,
            LogLevel = (Core.LogLevel)logLevel.GetHashCode(),
            Meta = null,// LogMetadata.GetMetadata(exception),
            Application = new()
            {
                Category = category,
                Name = applicationName,
                Machine = Environment.MachineName,
                Pid = Environment.ProcessId,
            }
        };
        provider.Write(logMessage);
    }
}
