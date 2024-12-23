using LogMQ.Core;
using ProtoBuf.Grpc;

namespace LogMQ.Services.Shared.LogManager;

public class LogService : ILogService
{
	public async Task<LogResponse> ShowLogAsync(ShowLogRequest request, CallContext context = default)
	{
		await Task.Delay(1);
		LogResponse response = new()
		{
			MessageList = [
				new LogMessage { Message = "Message1" },
				new LogMessage { Message = "Message2" },
				new LogMessage { Message = "Message3" },
				new LogMessage { Message = "Message4" },
				new LogMessage { Message = "Message5" },
				new LogMessage { Message = "Message6" },
				new LogMessage { Message = "Message7" },
				new LogMessage { Message = "Message8" },
				new LogMessage { Message = "Message9" },
				]
		};
		return response;
	}

	public Task<LogResponse> WatchLogAsync(WatchLogRequest request, CallContext context = default)
	{
		throw new NotImplementedException();
	}
}
