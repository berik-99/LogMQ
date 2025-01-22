using LogMQ.Providers.Contracts;
using Microsoft.Extensions.Logging;

namespace LogMQ.Extensions.Logging;

/// <summary>
/// A provider for creating instances of <see cref="LogMQLogger"/>, integrating LogMQ with the .NET logging system.
/// </summary>
internal sealed class LogMQLoggerProvider(ILogProvider provider, string applicationName) : ILoggerProvider
{
	/// <summary>
	/// Creates an instance of <see cref="LogMQLogger"/> for a specified category name.
	/// </summary>
	/// <param name="categoryName">The category name associated with the logger.</param>
	/// <returns>An instance of <see cref="LogMQLogger"/>.</returns>
	public ILogger CreateLogger(string categoryName) => new LogMQLogger(provider, applicationName, categoryName);

    /// <summary>
    /// Releases resources used by the provider.
    /// </summary>
    public void Dispose() => GC.SuppressFinalize(this);
}
