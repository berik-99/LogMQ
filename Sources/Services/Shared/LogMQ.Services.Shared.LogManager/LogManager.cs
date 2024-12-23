using Grpc.Net.Client;
using LogMQ.Core;
using ProtoBuf.Grpc.Client;

namespace LogMQ.Services.Shared.LogManager;

public class LogManager : ILogManager
{
	public async Task<List<LogMessage>> GetLogsByFilterAsync(string grpcAddress, string applicationName)
	{
		using var channel = GrpcChannel.ForAddress(grpcAddress);
		var client = channel.CreateGrpcService<ILogService>();

		var timeTo = DateTimeOffset.Now;
		var timeFrom = timeTo.AddSeconds(-30);
		var logs = await client.GetLogsByFilterAsync(new LogFilter { ApplicationName = applicationName, TimeFrom = timeFrom.DateTime, TimeTo = timeTo.DateTime, TimeOffset = timeTo.Offset });
		return logs;
	}

	public async Task<List<LogMessage>> GetLastLogsAsync(string grpcAddress, string applicationName, Guid guid, int count)
	{
		using var channel = GrpcChannel.ForAddress(grpcAddress);
		var client = channel.CreateGrpcService<ILogService>();
		var logs = await client.GetLastLogsAsync(new LastLogsFilter { ApplicationName = applicationName, StartFromId = guid, Count = count });
		return logs;
	}
}
