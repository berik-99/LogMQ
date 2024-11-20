using LogMQ.Contracts;

namespace LogMQ.Providers;

/// <summary>
/// Defines the contract for a fallback log provider used to handle errors and log messages 
/// when the primary log provider fails.
/// </summary>
public interface IFallbackLogProvider
{
	/// <summary>
	/// Writes a log message to the fallback storage or mechanism.
	/// This method is called when the primary log provider encounters an issue.
	/// </summary>
	/// <param name="message">The log message to be written.</param>
	void WriteFallback(LogMessage message);

	/// <summary>
	/// Records an error message and associated exception to the fallback storage or mechanism.
	/// </summary>
	/// <param name="message">A descriptive message explaining the error.</param>
	/// <param name="ex">The exception associated with the error, if available. Default is <c>null</c>.</param>
	void WriteError(string message, Exception ex = null);
}
