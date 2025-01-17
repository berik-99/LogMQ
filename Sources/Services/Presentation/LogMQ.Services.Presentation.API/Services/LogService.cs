using LogMQ.Core;
using LogMQ.Services.Shared.LogManager;
using LogMQ.Storage.Contracts;
using ProtoBuf.Grpc;

namespace LogMQ.Services.Presentation.API.Services;

public class LogService(ILogStorage storage) : ILogService
{
    public async Task<List<LogMessage>> GetLogsByFilterAsync(LogFilter filter, CallContext context = default)
    {
        return await storage.GetLogsAsync(filter);
        //await Task.Delay(1);
        //return [
        //	new LogMessage { Message = "Message1", Application = new() { Name = filter.ApplicationName }},
        //	new LogMessage { Message = "Message2", Application = new() { Name = filter.ApplicationName }},
        //	new LogMessage { Message = "Message3", Application = new() { Name = filter.ApplicationName }},
        //	new LogMessage { Message = "Message4", Application = new() { Name = filter.ApplicationName }},
        //	new LogMessage { Message = "Message5", Application = new() { Name = filter.ApplicationName }},
        //	new LogMessage { Message = "Message6", Application = new() { Name = filter.ApplicationName }},
        //	new LogMessage { Message = "Message7", Application = new() { Name = filter.ApplicationName }},
        //	new LogMessage { Message = "Message8", Application = new() { Name = filter.ApplicationName }},
        //	new LogMessage { Message = "Message9", Application = new() { Name = filter.ApplicationName }}
        //];
    }

    //public async Task<List<LogMessage>> GetLastLogsAsync(LastLogsFilter filter, CallContext context = default)
    //{
    //    throw new NotImplementedException();
    //    //return await storage.GetLastLogsAsync(filter);
    //}
}
