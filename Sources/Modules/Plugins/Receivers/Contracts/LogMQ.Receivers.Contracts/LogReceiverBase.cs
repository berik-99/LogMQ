using LogMQ.Storage.Contracts;
using Microsoft.Extensions.Hosting;

namespace LogMQ.Receivers.Contracts;

/// <summary>
/// Represents the base class for LogMQ receivers that process and store log messages.
/// </summary>
/// <param name="storage">An implementation of <see cref="ILogStorage"/> used for storing log messages.</param>
/// <exception cref="ArgumentNullException">Thrown if <paramref name="storage"/> is null.</exception>
public abstract class LogReceiverBase(ILogStorage storage) : BackgroundService
{
	/// <summary>
	/// Gets the storage instance used for persisting log messages.
	/// </summary>
	protected readonly ILogStorage Storage = storage ?? throw new ArgumentNullException(nameof(storage));
}
