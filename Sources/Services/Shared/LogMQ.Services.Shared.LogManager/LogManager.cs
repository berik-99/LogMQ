using Grpc.Net.Client;
using LogMQ.Core;
using ProtoBuf.Grpc.Client;

namespace LogMQ.Services.Shared.LogManager;

public class LogManager(string grpcAddress) : ILogService
{
    public async Task<List<string>> GetLogApplications()
    {
        using GrpcChannel channel = GrpcChannel.ForAddress(grpcAddress);
        ILogService client = channel.CreateGrpcService<ILogService>();
        return await client.GetLogApplications();
    }

    public async Task<List<LogMessage>> GetLogsAsync(LogFilter filter)
    {
        using GrpcChannel channel = GrpcChannel.ForAddress(grpcAddress);
        ILogService client = channel.CreateGrpcService<ILogService>();
        return await client.GetLogsAsync(filter);
    }

    public async Task<Wrapper<long>> GetTotalLogsCountAsync(Wrapper<string> applicationName)
    {
        using GrpcChannel channel = GrpcChannel.ForAddress(grpcAddress);
        ILogService client = channel.CreateGrpcService<ILogService>();
        return await client.GetTotalLogsCountAsync(applicationName);
    }
}