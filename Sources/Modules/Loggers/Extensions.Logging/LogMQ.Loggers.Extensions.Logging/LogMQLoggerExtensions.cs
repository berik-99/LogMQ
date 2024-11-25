using LogMQ.Providers.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LogMQ.Extensions.Logging;

	public static class LogMQLoggerExtensions
	{
		public static ILoggingBuilder AddLogMQ(
			this ILoggingBuilder builder,
			ILogProvider provider,
			string applicationName)
		{
			ArgumentNullException.ThrowIfNull(builder);
			ArgumentNullException.ThrowIfNull(provider);
			if (string.IsNullOrWhiteSpace(applicationName)) throw new ArgumentException("Application name cannot be empty", nameof(applicationName));

			builder.Services.AddSingleton<ILoggerProvider>(_ => new LogMQLoggerProvider(provider, applicationName));

			return builder;
		}
	}
