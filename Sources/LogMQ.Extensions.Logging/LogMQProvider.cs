using Microsoft.Extensions.Logging;

namespace LogMQ;
public class LogMQProvider : ILoggerProvider
{
    public ILogger CreateLogger(string categoryName)
    {
        return new MessageQueueLogger(categoryName, config.Value);
    }

    public void Dispose()
    {
        // Pulizia risorse, se necessario
    }
}

public class MessageQueueLogger(string categoryName, LogMQConfiguration config) : ILogger
{
    private readonly string _categoryName = categoryName;

    public IDisposable BeginScope<TState>(TState state)
    {
        return null; // Gestione degli scope opzionale
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return logLevel >= config.LogLevel;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
    {
        if (!IsEnabled(logLevel))
            return;

        if (config.LogLevel == LogLevel.None)
            return;

        // Costruisci il messaggio del log
        var message = formatter(state, exception);

        // Invia il messaggio alla Message Queue
        SendToMessageQueue(logLevel, _categoryName, message, exception);
    }

    private void SendToMessageQueue(LogLevel logLevel, string categoryName, string message, Exception exception)
    {
        // Logica per inviare il messaggio alla coda (simulato qui)
        Console.WriteLine($"[{config.QueueName}]-[{logLevel}] {_categoryName}: {message}");
        if (exception != null)
        {
            Console.WriteLine($"Exception: {exception}");
        }
    }
}

public class LogMQConfiguration
{
    public LogLevel LogLevel { get; set; }
    public string QueueName { get; set; }
}

