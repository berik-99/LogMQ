using Serilog.Configuration;
using System.Diagnostics;

namespace Serilog.Sinks.LogMQ;

/// <summary>
/// Provides extension methods for configuring custom log sinks to send logs to LogMQ's message broker and MSMQ.
/// </summary>
public static class LoggerSinkConfigurationExtensions
{
    /// <summary>
    /// Configures a custom log sink to send logs to LogMQ's internal message broker.
    /// The logs will be routed and processed by the LogMQ Service.
    /// </summary>
    /// <param name="loggerSinkConfiguration">The logger sink configuration to extend.</param>
    /// <param name="host">The host address of the LogMQ message broker.</param>
    /// <param name="port">The port number of the LogMQ message broker.</param>
    /// <param name="applicationName">The name of the application sending log messages. Defaults to the process name.</param>
    /// <param name="formatProvider">An optional <see cref="IFormatProvider"/> for formatting log messages.</param>
    /// <returns>
    /// A <see cref="LoggerConfiguration"/> object that allows further configuration of logging.
    /// </returns>
    public static LoggerConfiguration LogMQBrokerSink(
        this LoggerSinkConfiguration loggerSinkConfiguration,
        string host = "localhost",
        int port = 5563,
        string applicationName = null,
        IFormatProvider formatProvider = null)
    {
        applicationName ??= Process.GetCurrentProcess().ProcessName;
        return loggerSinkConfiguration.Sink(new LogMQBrokerSink(formatProvider, host, port, applicationName));
    }

#if Windows
    /// <summary>
    /// Configures a custom log sink to send logs to a specified MSMQ queue.
    /// The messages sent to the MSMQ queue will then be processed by the LogMQ Service.
    /// </summary>
    /// <param name="loggerSinkConfiguration">The logger sink configuration to extend.</param>
    /// <param name="queuePath">The path to the MSMQ queue.</param>
    /// <param name="applicationName">The name of the application sending log messages. Defaults to the process name.</param>
    /// <param name="formatProvider">An optional <see cref="IFormatProvider"/> for formatting log messages.</param>
    /// <returns>
    /// A <see cref="LoggerConfiguration"/> object that allows further configuration of logging.
    /// </returns>
    public static LoggerConfiguration LogMQMSMQSink(
        this LoggerSinkConfiguration loggerSinkConfiguration,
        string queuePath = @".\Private$\LogMQ_Queue",
        string applicationName = null,
        IFormatProvider formatProvider = null)
    {
        applicationName ??= Process.GetCurrentProcess().ProcessName;
        return loggerSinkConfiguration.Sink(new LogMQMSMQSink(formatProvider, queuePath, applicationName));
    }
#endif
}
