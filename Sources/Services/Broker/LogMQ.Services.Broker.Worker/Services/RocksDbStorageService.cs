using LogMQ.Core;
using LogMQ.Services.Shared.LogManager;
using LogMQ.Storage.Contracts;

namespace LogMQ.Services.Broker.Worker.Services;

public class RocksDbStorageService(ILogStorage storage) : ILogService
{
    public Task<List<string>> GetLogApplications() => storage.GetLogApplications();

    public Task<List<LogMessage>> GetLogsAsync(LogFilter filter) => storage.GetLogsAsync(filter);

    public Task<Wrapper<long>> GetTotalLogsCountAsync(Wrapper<string> applicationName) => storage.GetTotalLogsCountAsync(applicationName);
}
