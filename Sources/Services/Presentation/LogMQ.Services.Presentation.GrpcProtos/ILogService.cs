using LogMQ.Core;
using ProtoBuf.Grpc;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace LogMQ.Services.Presentation.GrpcContracts;

[DataContract]
public class LogResponse
{
	[DataMember(Order = 1)]
	public List<LogMessage> MessageList { get; set; }
}

[DataContract]
public class ShowLogRequest
{
	[DataMember(Order = 1)]
	public string ApplicationName { get; set; }
}

[DataContract]
public class WatchLogRequest
{
	[DataMember(Order = 1)]
	public string ApplicationName { get; set; }

	[DataMember(Order = 2)]
	public Guid GetAfterId { get; set; }
}

[ServiceContract]
public interface ILogService
{
	[OperationContract]
	Task<LogResponse> ShowLogAsync(ShowLogRequest request, CallContext context = default);

	[OperationContract]
	Task<LogResponse> WatchLogAsync(WatchLogRequest request, CallContext context = default);
}
