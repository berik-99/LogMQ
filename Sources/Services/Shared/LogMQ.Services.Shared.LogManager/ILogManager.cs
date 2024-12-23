
using LogMQ.Core;

namespace LogMQ.Services.Shared.LogManager;

public interface ILogManager
{
	Task<List<LogMessage>> ShowLogAsync(string grpcAddress, string applicationName);
}
