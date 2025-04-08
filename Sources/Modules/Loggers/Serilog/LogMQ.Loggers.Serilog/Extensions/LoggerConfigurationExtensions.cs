using System.Diagnostics;
using LogMQ.Loggers.Serilog.Enrichers;
using LogMQ.Providers.Contracts;
using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;

#pragma warning disable IDE0130
namespace LogMQ.Loggers.Serilog;
#pragma warning restore IDE0130

/// <summary>
/// Provides extension methods for configuring Serilog to integrate with LogMQ.
/// These methods allow developers to set up sinks, enrichers, and pre-configured
/// logging stacks for seamless integration with the LogMQ message broker.
/// </summary>
public static class LoggerConfigurationExtensions
{

    /// <summary>
    /// The default category name for log messages.
    /// </summary>
    /// <remarks>
    /// This value is used when no specific category is provided for a log entry.
    /// </remarks>
    private const string DefaultCategory = "Generic";

    /// <summary>
    /// The default application name derived from the current process name.
    /// </summary>
    /// <remarks>
    /// This value is determined dynamically at runtime based on the name of the executing process.
    /// </remarks>
    private static string DefualtApplicationname => Process.GetCurrentProcess().ProcessName;

    /// <summary>
    /// Configures a custom Serilog sink to send log events to LogMQ message broker.
    /// This sink routes log messages to LogMQ for structured storage and processing.
    /// </summary>
    /// <param name="sinkConfiguration">The <see cref="LoggerSinkConfiguration"/> to extend.</param>
    /// <param name="logProvider">
    /// An instance of <see cref="ILogProvider"/> responsible for managing log delivery to LogMQ.
    /// </param>
    /// <param name="category">
    /// A category representing the type of application generating the logs (e.g., 'Console', 'Desktop', 'Web').
    /// Defaults to 'Generic' if not provided.
    /// </param>
    /// <param name="applicationName">
    /// The name of the application generating the logs. If <c>null</c> or empty, it defaults to the current process name.
    /// </param>
    /// <param name="restrictedToMinimumLevel">
    /// The minimum log level for events to be written to this sink. Defaults to <see cref="LogEventLevel.Verbose"/>.
    /// </param>
    /// <param name="levelSwitch">
    /// An optional <see cref="LoggingLevelSwitch"/> to dynamically control the minimum log level at runtime.
    /// Overrides <paramref name="restrictedToMinimumLevel"/> if specified.
    /// </param>
    /// <returns>
    /// A <see cref="LoggerConfiguration"/> object for further configuration.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="sinkConfiguration"/> is <c>null</c>.</exception>
    public static LoggerConfiguration LogMQ(
        this LoggerSinkConfiguration sinkConfiguration,
        ILogProvider logProvider,
        string category = DefaultCategory,
        string applicationName = null,
        LogEventLevel restrictedToMinimumLevel = LogEventLevel.Verbose,
        LoggingLevelSwitch levelSwitch = null)
    {
        ArgumentNullException.ThrowIfNull(sinkConfiguration);
        category = string.IsNullOrWhiteSpace(category) ? DefaultCategory : category;
        applicationName = string.IsNullOrWhiteSpace(applicationName) ? DefualtApplicationname : applicationName;
        return sinkConfiguration.Sink(new LogMQSink(logProvider, applicationName, category, restrictedToMinimumLevel, levelSwitch));
    }

    /// <summary>
    /// Configures Serilog to enrich log events with LogMQ-specific metadata.
    /// Adds contextual properties such as file name, class name, method details, and exception information.
    /// </summary>
    /// <param name="enrichmentConfiguration">
    /// The <see cref="LoggerEnrichmentConfiguration"/> to extend with LogMQ-specific enrichers.
    /// </param>
    /// <returns>
    /// A <see cref="LoggerConfiguration"/> object for further configuration.
    /// </returns>
    public static LoggerConfiguration WithLogMQMetadata(this LoggerEnrichmentConfiguration enrichmentConfiguration)
        => enrichmentConfiguration.With(new MetadataEnricher());

    public static LoggerConfiguration WithLogMQInfo(this LoggerEnrichmentConfiguration enrichmentConfiguration)
        => enrichmentConfiguration.With(new LogInfoEnricher());

    /// <summary>
    /// Configures a pre-defined logging stack for LogMQ, including metadata enrichment and sink configuration.
    /// This method simplifies the setup process for applications using LogMQ.
    /// </summary>
    /// <param name="loggerConfiguration">
    /// The <see cref="LoggerConfiguration"/> to extend with LogMQ's logging stack.
    /// </param>
    /// <param name="logProvider">
    /// An instance of <see cref="ILogProvider"/> responsible for managing log delivery to LogMQ.
    /// </param>
    /// <param name="category">
    /// A category representing the type of application generating the logs. Defaults to 'Generic' if not provided.
    /// </param>
    /// <param name="applicationName">
    /// The name of the application generating the logs. Defaults to the current process name if not provided.
    /// </param>
    /// <param name="restrictedToMinimumLevel">
    /// The minimum log level for events to be written. Defaults to <see cref="LogEventLevel.Verbose"/>.
    /// </param>
    /// <param name="levelSwitch">
    /// An optional <see cref="LoggingLevelSwitch"/> for dynamic control of the log level at runtime.
    /// </param>
    /// <returns>
    /// A <see cref="LoggerConfiguration"/> object for further customization.
    /// </returns>
    public static LoggerConfiguration UseLogMQStack(
        this LoggerConfiguration loggerConfiguration,
        ILogProvider logProvider,
        string category = DefaultCategory,
        string applicationName = null,
        LogEventLevel restrictedToMinimumLevel = LogEventLevel.Verbose,
        LoggingLevelSwitch levelSwitch = null)
    {
        category = string.IsNullOrWhiteSpace(category) ? DefaultCategory : category;
        applicationName = string.IsNullOrWhiteSpace(applicationName) ? DefualtApplicationname : applicationName;
        loggerConfiguration.Enrich.WithLogMQInfo();
        loggerConfiguration.Enrich.WithLogMQMetadata();
        loggerConfiguration.WriteTo.LogMQ(logProvider, category, applicationName, restrictedToMinimumLevel, levelSwitch);
        return loggerConfiguration;
    }
}
