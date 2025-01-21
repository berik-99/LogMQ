using Grpc.Net.Client;
using LogMQ.Core;
using LogMQ.Storage.Contracts;
using ProtoBuf.Grpc.Client;

namespace LogMQ.Services.Shared.LogManager;

public class LogManager(string grpcAddress) : ILogStorage
{
    public async Task<List<string>> GetLogApplications()
    {
        using GrpcChannel channel = GrpcChannel.ForAddress(grpcAddress);
        ILogStorage client = channel.CreateGrpcService<ILogStorage>();
        return await client.GetLogApplications();
    }

    public async Task<List<LogMessage>> GetLogsAsync(LogFilter filter)
    {
        using GrpcChannel channel = GrpcChannel.ForAddress(grpcAddress);
        ILogStorage client = channel.CreateGrpcService<ILogStorage>();
        return await client.GetLogsAsync(filter);
    }

    public async Task<Wrapper<long>> GetTotalLogsCountAsync(Wrapper<string> applicationName)
    {
        using GrpcChannel channel = GrpcChannel.ForAddress(grpcAddress);
        ILogStorage client = channel.CreateGrpcService<ILogStorage>();
        return await client.GetTotalLogsCountAsync(applicationName);
    }

    public async Task WriteLogMessageAsync(LogMessage logMessage)
    {
        using GrpcChannel channel = GrpcChannel.ForAddress(grpcAddress);
        ILogStorage client = channel.CreateGrpcService<ILogStorage>();
        await client.WriteLogMessageAsync(logMessage);
    }
}