﻿using Serilog.Configuration;

namespace Serilog.Sinks.LogMQ;

/// <summary>
/// Provides extension methods for configuring custom log sinks to send logs to LogMQ's message broker and MSMQ.
/// </summary>
public static class LoggerSinkConfigurationExtensions
{
	/// <summary>
	/// Configures a custom log sink to send logs to LogMQ's internal message broker.
	/// The logs will then be routed and processed by the LogMQ Service.
	/// </summary>
	/// <param name="loggerSinkConfiguration">The logger sink configuration to extend.</param>
	/// <param name="host">The host address of the LogMQ message broker.</param>
	/// <param name="port">The port number of the LogMQ message broker.</param>
	/// <param name="applicationName">The name of the application sending log messages.</param>
	/// <returns>A <see cref="LoggerConfiguration"/> object that allows further configuration of logging.</returns>
	public static LoggerConfiguration LogMQBrokerSink(this LoggerSinkConfiguration loggerSinkConfiguration, string host, int port, string applicationName)
	{
		return loggerSinkConfiguration.Sink(new LogMQBrokerSink(null, host, port, applicationName));
	}

	/// <summary>
	/// Configures a custom log sink to send logs to LogMQ's internal message broker without specifying an application name.
	/// The logs will then be routed and processed by the LogMQ Service.
	/// </summary>
	/// <param name="loggerSinkConfiguration">The logger sink configuration to extend.</param>
	/// <param name="host">The host address of the LogMQ message broker.</param>
	/// <param name="port">The port number of the LogMQ message broker.</param>
	/// <returns>A <see cref="LoggerConfiguration"/> object that allows further configuration of logging.</returns>
	public static LoggerConfiguration LogMQBrokerSink(this LoggerSinkConfiguration loggerSinkConfiguration, string host, int port)
	{
		return loggerSinkConfiguration.LogMQBrokerSink(host, port, null);
	}

#if Windows

	public static LoggerConfiguration LogMQMSMQSink(this LoggerSinkConfiguration loggerSinkConfiguration)
	{
		return loggerSinkConfiguration.Sink(new LogMQMSMQSink());
	}
#endif
}