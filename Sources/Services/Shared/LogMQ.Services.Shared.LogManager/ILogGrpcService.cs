using System.ServiceModel;
using LogMQ.Core;
using LogMQ.Services.Shared.LogManager.Filters;

namespace LogMQ.Services.Shared.LogManager;

[ServiceContract]
public interface ILogGrpcService
{
    //Return logs filtered by filter
    [OperationContract]
    Task<List<LogMessage>> GetLogsAsync(SearchFilter filter);

    //Return log count filtered by filter
    [OperationContract]
    Task<Wrapper<ulong>> CountLogsAsync(SearchFilter filter);

    //Delete all logs by filter and return the count of deleted logs
    [OperationContract]
    Task<Wrapper<ulong>> ClearLogsAsync(ClearFilter filter);

    //Retun the list of logging applications
    [OperationContract]
    Task<List<string>> GetLogApplications();
}