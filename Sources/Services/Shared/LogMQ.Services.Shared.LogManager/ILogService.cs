using LogMQ.Core;
using ProtoBuf;
using ProtoBuf.Grpc;
using System.ServiceModel;

namespace LogMQ.Services.Shared.LogManager;

[ProtoContract]
public class LastLogsFilter
{
	[ProtoMember(1)]
	public string ApplicationName { get; set; }

	[ProtoMember(2)]
	public Guid StartFromId { get; set; }

	[ProtoMember(3)]
	public int Count { get; set; }
}

[ServiceContract]
public interface ILogService
{
	[OperationContract]
	Task<List<LogMessage>> GetLogsByFilterAsync(LogFilter filter, CallContext context = default);

	[OperationContract]
	Task<List<LogMessage>> GetLastLogsAsync(LastLogsFilter filter, CallContext context = default);
}
