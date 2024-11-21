using LogMQ.Core;

namespace LogMQ.Providers.Contracts;

/// <summary>
/// Defines the contract for a LogMQ log provider.
/// </summary>
public interface ILogProvider
{
	/// <summary>
	/// Gets the format provider used for log message formatting.
	/// </summary>
	IFormatProvider FormatProvider { get; }

	/// <summary>
	/// Gets the fallback logger used for handling errors and fallback logging.
	/// </summary>
	IFallbackLogProvider FallbackLogger { get; }

	/// <summary>
	/// Sends a log message to the LogMQ Broker synchronously.
	/// If an error occurs, the message is handled by the fallback logger.
	/// </summary>
	/// <param name="message">The log message to be sent.</param>
	void Write(LogMessage message);
}