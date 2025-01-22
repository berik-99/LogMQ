using System.ServiceModel;
using LogMQ.Core;

namespace LogMQ.Services.Shared.LogManager;

[ServiceContract]
public interface ILogService
{
    [OperationContract]
    Task<List<LogMessage>> GetLogsAsync(LogFilter filter);

    [OperationContract]
    Task<Wrapper<long>> GetTotalLogsCountAsync(Wrapper<string> applicationName);

    [OperationContract]
    Task<List<string>> GetLogApplications();
}
