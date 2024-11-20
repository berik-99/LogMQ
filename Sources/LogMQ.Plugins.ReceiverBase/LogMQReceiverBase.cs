using LogMQ.Plugins.Storage.Contracts;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LogMQ.Plugins.Receivers.Contracts;

public abstract class LogMQReceiverBase(ILogMQStorage storage) : BackgroundService
{
	protected readonly ILogMQStorage Storage = storage ?? throw new ArgumentNullException(nameof(storage));
}
