using Serilog.Core;
using Serilog.Events;

namespace LogMQ.Loggers.Serilog.Enrichers;

internal sealed class LogInfoEnricher : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        LogEventProperty property = propertyFactory.CreateProperty(nameof(Guid), Guid.NewGuid());
        logEvent.AddPropertyIfAbsent(property);
    }
}
