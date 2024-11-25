using LogMQ.Loggers.Serilog.Extensions;
using Serilog.Core;
using Serilog.Events;

namespace LogMQ.Loggers.Serilog;

public class LogMQEnricher : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var meta = logEvent.GetMetadata();

        var properties = new[]
{
            propertyFactory.CreateProperty(nameof(meta.File), meta.File),
            propertyFactory.CreateProperty(nameof(meta.Class), meta.Class),
            propertyFactory.CreateProperty(nameof(meta.MethodName), meta.MethodName),
            propertyFactory.CreateProperty(nameof(meta.MethodSignature), meta.MethodSignature),
            propertyFactory.CreateProperty(nameof(meta.Row), meta.Row),
            propertyFactory.CreateProperty(nameof(meta.Exception), meta.Exception)
        };

        foreach (var property in properties)
        {
            logEvent.AddPropertyIfAbsent(property);
        }
    }
}
