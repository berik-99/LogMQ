using LogMQ.Core;
using LogMQ.Services.Presentation.GrpcContracts;
using ProtoBuf.Grpc;
using System.Collections.Concurrent;

namespace LogMQ.Services.Presentation.API.Services;

public class LogService : ILogService
{
	public LogService()
	{
		Task.Run(FillProvider);
	}


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

	private readonly ConcurrentDictionary<Guid, string> fakeProvider = new();

	private void FillProvider()
	{
		int i = 0;
		while (true)
		{
			string msg = $"Message #{i}";
			fakeProvider.TryAdd(Guid.NewGuid(), msg);
			Console.WriteLine($"MESSAGE GENERATED: {msg}");
			Thread.Sleep(250);
			i++;
		}
	}

	public Task<LogResponse> WatchLogAsync(WatchLogRequest request, CallContext context = default)
	{
		var guid = request.GetAfterId == Guid.Empty ? fakeProvider.Keys.FirstOrDefault() : fakeProvider.Keys.FirstOrDefault(x => x == request.GetAfterId);
		var messages = fakeProvider.Where(x => x.Key.CompareTo(guid) > 0).Select(x => new LogMessage { Message = x.Value }).ToList();
		LogResponse response = new()
		{
			MessageList = messages
		};
		return Task.FromResult(response);
	}
}
