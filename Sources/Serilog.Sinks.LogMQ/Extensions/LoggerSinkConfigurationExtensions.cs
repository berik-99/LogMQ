using Serilog.Configuration;
using Serilog.Core;
using System.Diagnostics;

namespace Serilog.Sinks.LogMQ;

/// <summary>
/// Provides extension methods for configuring custom log sinks to send logs to LogMQ's message broker and MSMQ.
/// </summary>
public static class LoggerSinkConfigurationExtensions
{
    private const string defultTcpHost = "localhost";
    private const int defultTcpPort = 5563;
    private const string defultQueuePath = @".\Private$\LogMQ_Queue";
    private const string defultCategory = "Generic";
    private static readonly string defultApplicationName = Process.GetCurrentProcess().ProcessName;
    private static readonly ILogEventSink defaultFallbackSink = new LoggerConfiguration().WriteTo.Console().CreateLogger();

    /// <summary>
    /// Configures a custom log sink to send logs to LogMQ's internal message broker.
    /// The logs will be routed and processed by the LogMQ Service.
    /// </summary>
    /// <param name="loggerSinkConfiguration">The logger sink configuration to extend.</param>
    /// <param name="host">The host address of the LogMQ message broker. Defaults to 'localhost'.</param>
    /// <param name="port">The port number of the LogMQ message broker. Defaults to 5563.</param>
    /// <param name="category">The log category (e.g., 'Generic'). Defaults to 'Generic'.</param>
    /// <param name="applicationName">The name of the application sending log messages. Defaults to the current process name.</param>
    /// <param name="formatProvider">An optional <see cref="IFormatProvider"/> for formatting log messages.</param>
    /// <param name="fallbackLogger">An optional fallback logger in case of errors. Defaults to a console logger.</param>
    /// <returns>
    /// A <see cref="LoggerConfiguration"/> object that allows further configuration of logging.
    /// </returns>
    public static LoggerConfiguration LogMQBrokerSink(
        this LoggerSinkConfiguration loggerSinkConfiguration,
        string host = defultTcpHost,
        int port = defultTcpPort,
        string category = defultCategory,
        string applicationName = null,
        IFormatProvider formatProvider = null,
        ILogEventSink fallbackLogger = null)
    {
        category = string.IsNullOrWhiteSpace(category) ? defultCategory : category;
        applicationName = string.IsNullOrWhiteSpace(applicationName) ? defultApplicationName : applicationName;
        fallbackLogger ??= defaultFallbackSink;
        return loggerSinkConfiguration.Sink(new LogMQBrokerSink(formatProvider, host, port, category, applicationName, fallbackLogger));
    }

#if Windows

    /// <summary>
    /// Configures a custom log sink to send logs to a specified MSMQ queue.
    /// The messages sent to the MSMQ queue will then be processed by the LogMQ Service.
    /// </summary>
    /// <param name="loggerSinkConfiguration">The logger sink configuration to extend.</param>
    /// <param name="queuePath">The path to the MSMQ queue. Defaults to '.\Private$\LogMQ_Queue'.</param>
    /// <param name="category">The log category (e.g., 'Generic'). Defaults to 'Generic'.</param>
    /// <param name="applicationName">The name of the application sending log messages. Defaults to the current process name.</param>
    /// <param name="formatProvider">An optional <see cref="IFormatProvider"/> for formatting log messages.</param>
    /// <param name="fallbackLogger">An optional fallback logger in case of errors. Defaults to a console logger.</param>
    /// <returns>
    /// A <see cref="LoggerConfiguration"/> object that allows further configuration of logging.
    /// </returns>
    public static LoggerConfiguration LogMQMSMQSink(
        this LoggerSinkConfiguration loggerSinkConfiguration,
        string queuePath = defultQueuePath,
        string category = defultCategory,
        string applicationName = null,
        IFormatProvider formatProvider = null,
        ILogEventSink fallbackLogger = null)
    {
        queuePath = string.IsNullOrWhiteSpace(queuePath) ? defultQueuePath : queuePath;
        category = string.IsNullOrWhiteSpace(category) ? defultCategory : category;
        applicationName = string.IsNullOrWhiteSpace(applicationName) ? defultApplicationName : applicationName;
        fallbackLogger ??= defaultFallbackSink;
        return loggerSinkConfiguration.Sink(new LogMQMSMQSink(formatProvider, queuePath, applicationName, category, fallbackLogger));
    }

#endif
}
