using Grpc.Net.Client;
using LogMQ.Core;
using LogMQ.Services.Shared.LogManager.Filters;
using ProtoBuf.Grpc.Client;

namespace LogMQ.Services.Shared.LogManager;

public class LogManager(string grpcAddress) : ILogGrpcService
{
    public async Task<List<string>> GetLogApplications()
    {
        using GrpcChannel channel = GrpcChannel.ForAddress(grpcAddress);
        ILogGrpcService client = channel.CreateGrpcService<ILogGrpcService>();
        return await client.GetLogApplications();
    }

    public async Task<List<LogMessage>> GetLogsAsync(SearchFilter filter)
    {
        using GrpcChannel channel = GrpcChannel.ForAddress(grpcAddress);
        ILogGrpcService client = channel.CreateGrpcService<ILogGrpcService>();
        return await client.GetLogsAsync(filter);
    }

    public async Task<Wrapper<ulong>> ClearLogsAsync(ClearFilter filter)
    {
        using GrpcChannel channel = GrpcChannel.ForAddress(grpcAddress);
        ILogGrpcService client = channel.CreateGrpcService<ILogGrpcService>();
        return await client.ClearLogsAsync(filter);
    }

    public async Task<Wrapper<ulong>> CountLogsAsync(SearchFilter filter)
    {
        using GrpcChannel channel = GrpcChannel.ForAddress(grpcAddress);
        ILogGrpcService client = channel.CreateGrpcService<ILogGrpcService>();
        return await client.CountLogsAsync(filter);
    }

    public async Task<Wrapper<ulong>> MergeLogsAsync(MergeFilter filter)
    {
        using GrpcChannel channel = GrpcChannel.ForAddress(grpcAddress);
        ILogGrpcService client = channel.CreateGrpcService<ILogGrpcService>();
        return await client.MergeLogsAsync(filter);
    }
}