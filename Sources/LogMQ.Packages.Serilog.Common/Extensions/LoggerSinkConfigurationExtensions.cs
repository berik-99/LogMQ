using LogMQ.Providers;
using Serilog;
using Serilog.Configuration;
using System.Diagnostics;

namespace LogMQ.Serilog.Extensions;

/// <summary>
/// Provides extension methods for configuring custom log sinks to send logs to LogMQ's message broker and MSMQ.
/// </summary>
public static class LoggerSinkConfigurationExtensions
{
	private const string DefaultTcpHost = "localhost";
	private const int DefaultTcpPort = 5563;
	private const string DefaultQueuePath = @".\Private$\LogMQ_Queue";
	private const string DefaultCategory = "Generic";
	private static readonly string DefaultApplicationName = Process.GetCurrentProcess().ProcessName;
	//private static readonly ILogEventSink DefaultFallbackSink = new LoggerConfiguration().WriteTo.Console().CreateLogger();

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
	public static LoggerConfiguration LogMQ(
		this LoggerSinkConfiguration loggerSinkConfiguration,
		ILogProvider logProvider,
		string category = DefaultCategory,
		string applicationName = null)
	{
		ArgumentNullException.ThrowIfNull(loggerSinkConfiguration);
		category = string.IsNullOrWhiteSpace(category) ? DefaultCategory : category;
		applicationName = string.IsNullOrWhiteSpace(applicationName) ? DefaultApplicationName : applicationName;
		return loggerSinkConfiguration.Sink(new LogMQSink(logProvider, category, applicationName));
	}
}
