using LogMQ.Loggers.Serilog.Extensions;
using LogMQ.Providers.Contracts;
using Serilog.Core;
using Serilog.Events;

namespace LogMQ.Loggers.Serilog;

/// <summary>
/// A custom Serilog sink that sends log events to a specified <see cref="ILogProvider"/>.
/// </summary>
/// <remarks>
/// This sink allows integration with the LogMQ infrastructure by forwarding Serilog log events
/// as structured log messages to the provided <see cref="ILogProvider"/>. It supports filtering
/// logs based on a minimum level and an optional <see cref="LoggingLevelSwitch"/>.
/// </remarks>
/// <param name="provider">The log provider responsible for handling the delivery of log messages.</param>
/// <param name="applicationName">The name of the application sending the log message.</param>
/// <param name="category">The category of the log, used to classify messages.</param>
/// <param name="restrictedToMinimumLevel">
/// The minimum log level required to process events. Defaults to <see cref="LogEventLevel.Verbose"/>.
/// </param>
/// <param name="levelSwitch">
/// An optional <see cref="LoggingLevelSwitch"/> to dynamically control the minimum log level at runtime.
/// </param>
internal sealed class LogMQSink(ILogProvider provider,
                                string applicationName,
                                string category,
                                LogEventLevel restrictedToMinimumLevel = LogEventLevel.Verbose,
                                LoggingLevelSwitch levelSwitch = null) : ILogEventSink
{
    /// <summary>
    /// Processes and sends a log event to the configured <see cref="ILogProvider"/>.
    /// </summary>
    /// <param name="logEvent">The log event to process and deliver.</param>
    /// <remarks>
    /// The method applies filtering based on the minimum log level specified at construction or via
    /// the <see cref="LoggingLevelSwitch"/>. If the log level is below the threshold, the event is ignored.
    /// The log event is converted to a <c>LogMessage</c> using the provider's formatter.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="logEvent"/> is <c>null</c>.</exception>
    public void Emit(LogEvent logEvent)

    {
        if (levelSwitch == null && logEvent.Level < restrictedToMinimumLevel)
            return;
        if (levelSwitch != null && logEvent.Level < levelSwitch.MinimumLevel)
            return;
        provider.Write(logEvent.ToLogMessage(provider.FormatProvider, applicationName, category));
    }
}
