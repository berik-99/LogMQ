using LogMQ.Plugins.Storage.Contracts;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LogMQ.Plugins.Receivers.Contracts;

public abstract class LogMQReceiverBase(ILogger logger, ILogMQStorage rdb) : BackgroundService;