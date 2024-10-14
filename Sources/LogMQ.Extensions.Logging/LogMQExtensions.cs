using Microsoft.Extensions.Logging;

namespace LogMQ.Extensions.Logging;

public static class LogMQExtensions
{
    // Metodo di estensione che aggiunge il provider con configurazione da IOptions
    public static ILoggingBuilder AddLogMQ(this ILoggingBuilder builder)
    {
        // Registra la configurazione per essere iniettata automaticamente nel provider
        builder.Services.AddSingleton<ILoggerProvider, LogMQProvider>();

        return builder;
    }

    // Metodo di estensione che accetta una configurazione inline
    public static ILoggingBuilder AddLogMQ(this ILoggingBuilder builder, Action<LogMQConfiguration> configure)
    {
        // Registra la configurazione fornita dall'utente
        builder.Services.Configure(configure);

        // Registra il provider
        builder.Services.AddSingleton<ILoggerProvider, LogMQProvider>();

        return builder;
    }
}
