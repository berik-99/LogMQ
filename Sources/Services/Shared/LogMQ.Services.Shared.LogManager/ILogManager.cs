
using LogMQ.Core;

namespace LogMQ.Services.Shared.LogManager;

public interface ILogManager
{
	Task<List<LogMessage>> GetLogsByFilterAsync(string grpcAddress, string applicationName);
	Task<List<LogMessage>> GetLastLogsAsync(string grpcAddress, string applicationName, Guid guid, int count);
}
