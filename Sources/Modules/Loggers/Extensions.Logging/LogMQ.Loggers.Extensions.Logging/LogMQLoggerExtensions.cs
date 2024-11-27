using LogMQ.Providers.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LogMQ.Extensions.Logging;

/// <summary>
/// Extension methods for integrating LogMQ with the .NET logging system.
/// </summary>
public static class LogMQLoggerExtensions
{
	/// <summary>
	/// Adds a LogMQ-based logger to the logging system.
	/// </summary>
	/// <param name="builder">The <see cref="ILoggingBuilder"/> to configure.</param>
	/// <param name="provider">The <see cref="ILogProvider"/> used to write log messages to LogMQ.</param>
	/// <param name="applicationName">The name of the application generating the logs.</param>
	/// <returns>The <see cref="ILoggingBuilder"/> for chaining additional configuration.</returns>
	/// <exception cref="ArgumentNullException">Thrown if <paramref name="builder"/> or <paramref name="provider"/> is <c>null</c>.</exception>
	/// <exception cref="ArgumentException">Thrown if <paramref name="applicationName"/> is <c>null</c>, empty, or consists only of whitespace.</exception>
	public static ILoggingBuilder AddLogMQ(
		this ILoggingBuilder builder,
		ILogProvider provider,
		string applicationName)
	{
		ArgumentNullException.ThrowIfNull(builder);
		ArgumentNullException.ThrowIfNull(provider);
		if (string.IsNullOrWhiteSpace(applicationName))
			throw new ArgumentException("Application name cannot be empty", nameof(applicationName));

		builder.Services.AddSingleton<ILoggerProvider>(_ => new LogMQLoggerProvider(provider, applicationName));

		return builder;
	}
}
