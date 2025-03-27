using LogMQ.Core;
using LogMQ.Services.Shared.LogManager;

namespace LogMQ.Services.Broker.Worker.Services;

public class DuckDBStorageService(ILogger<DuckDBStorageService> logger, DuckDBStorageConfiguration config) : Receivers.Contracts.ILogStorage, ILogService
{
    public Task<List<string>> GetLogApplications()
    {
        logger.LogInformation("Request received for: GetLogApplications");
        return Task.FromResult<List<string>>(null);
    }

    public Task<List<LogMessage>> GetLogsAsync(LogFilter filter)
    {
        logger.LogInformation("Request received for: GetLogsAsync");
        return Task.FromResult<List<LogMessage>>(null);
    }

    public Task<Wrapper<long>> GetTotalLogsCountAsync(Wrapper<string> applicationName)
    {
        logger.LogInformation("Request received for: GetTotalLogsCountAsync");
        return Task.FromResult<Wrapper<long>>(null);
    }

    public Task WriteLogMessageAsync(LogMessage logMessage)
    {
        logger.LogInformation("Message received: {Message}", logMessage.Message);
        return Task.CompletedTask;
    }
}
