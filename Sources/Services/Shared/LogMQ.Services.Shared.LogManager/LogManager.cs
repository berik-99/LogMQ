using Grpc.Net.Client;
using LogMQ.Core;
using ProtoBuf.Grpc.Client;

namespace LogMQ.Services.Shared.LogManager;

public class LogManager : ILogManager
{
	public async Task<List<LogMessage>> ShowLogAsync(string grpcAddress, string applicationName)
	{
		using var channel = GrpcChannel.ForAddress(grpcAddress);
		var client = channel.CreateGrpcService<ILogService>();
		var reply = await client.ShowLogAsync(new ShowLogRequest { ApplicationName = applicationName });
		return reply.MessageList;
	}
}
