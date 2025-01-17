using System.ServiceModel;
using LogMQ.Core;
using ProtoBuf.Grpc;

namespace LogMQ.Services.Shared.LogManager;

[ServiceContract]
public interface ILogService
{
    [OperationContract]
    Task<List<LogMessage>> GetLogsByFilterAsync(LogFilter filter, CallContext context = default);
}
