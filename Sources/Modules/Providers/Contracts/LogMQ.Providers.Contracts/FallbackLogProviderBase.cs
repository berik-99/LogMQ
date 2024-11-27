using LogMQ.Core;

namespace LogMQ.Providers.Contracts;

public abstract class FallbackLogProviderBase : IFallbackLogProvider
{
	/// <inheritdoc />
	public abstract void WriteFallback(LogMessage message);

	/// <inheritdoc />
	public abstract void Write(LogLevel logLevel, string message, Exception ex = null);

	/// <inheritdoc />
	public void WriteTrace(string message) => Write(LogLevel.Trace, message);

	/// <inheritdoc />
	public void WriteDebug(string message) => Write(LogLevel.Debug, message);

	/// <inheritdoc />
	public void WriteInformation(string message) => Write(LogLevel.Information, message);

	/// <inheritdoc />
	public void WriteWarning(string message, Exception ex = null) => Write(LogLevel.Warning, message);

	/// <inheritdoc />
	public void WriteError(string message, Exception ex = null) => Write(LogLevel.Error, message, ex);

	/// <inheritdoc />
	public void WriteCritical(string message, Exception ex = null) => Write(LogLevel.Critical, message, ex);
}
