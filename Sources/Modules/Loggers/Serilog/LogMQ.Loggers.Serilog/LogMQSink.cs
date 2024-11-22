using LogMQ.Loggers.Serilog.Extensions;
using LogMQ.Providers.Contracts;
using Serilog.Core;
using Serilog.Events;

namespace LogMQ.Loggers.Serilog;

internal sealed class LogMQSink(ILogProvider provider,
                                string applicationName,
                                string category,
                                LogEventLevel restrictedToMinimumLevel = LogEventLevel.Verbose,
                                LoggingLevelSwitch levelSwitch = null) : ILogEventSink
{
    public void Emit(LogEvent logEvent)
    {
        if (levelSwitch == null && logEvent.Level < restrictedToMinimumLevel)
            return;
        if (levelSwitch != null && logEvent.Level < levelSwitch.MinimumLevel)
            return;
        provider.Write(logEvent.ToLogMessage(provider.FormatProvider, applicationName, category));
    }
}
