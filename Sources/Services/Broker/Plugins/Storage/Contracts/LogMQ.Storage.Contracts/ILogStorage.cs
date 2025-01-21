using System.ServiceModel;
using LogMQ.Core;

namespace LogMQ.Storage.Contracts;

//TODO: Add documentation

[ServiceContract]
public interface ILogStorage
{
    [OperationContract]
    Task WriteLogMessageAsync(LogMessage logMessage);

    [OperationContract]
    Task<List<LogMessage>> GetLogsAsync(LogFilter filter);

    [OperationContract]
    Task<Wrapper<long>> GetTotalLogsCountAsync(Wrapper<string> applicationName);

    [OperationContract]
    Task<List<string>> GetLogApplications();
}