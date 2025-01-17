namespace LogMQ.Services.Shared.LogManager;

//public class LogManager : ILogManager
//{
//    public async Task<List<LogMessage>> GetLogsByFilterAsync(string grpcAddress, string applicationName)

//    {
//        using GrpcChannel channel = GrpcChannel.ForAddress(grpcAddress);
//        ILogService client = channel.CreateGrpcService<ILogService>();

//        var now = UniversalDateTime.Now;
//        List<LogMessage> logs = await GetLogsAsync(new LogFilter() { ApplicationName = appName, Count = 30, TimeFrom = now.AddHours(-1), TimeTo = now });
//        return logs;
//    }

//    public async Task<List<LogMessage>> GetLogsAsync(string grpcAddress, string applicationName, Guid guid, int count)
//    {
//        using GrpcChannel channel = GrpcChannel.ForAddress(grpcAddress);
//        ILogService client = channel.CreateGrpcService<ILogService>();
//        List<LogMessage> logs = await client.GetLastLogsAsync(new LastLogsFilter { ApplicationName = applicationName, StartFromId = guid, Count = count });
//        return logs;
//    }

//    //public async Task<List<LogMessage>> GetLastLogsAsync(string grpcAddress, string applicationName, Guid guid, int count)
//    //{
//    //    using GrpcChannel channel = GrpcChannel.ForAddress(grpcAddress);
//    //    ILogService client = channel.CreateGrpcService<ILogService>();
//    //    List<LogMessage> logs = await client.GetLastLogsAsync(new LastLogsFilter { ApplicationName = applicationName, StartFromId = guid, Count = count });
//    //    return logs;
//    //}
//}
