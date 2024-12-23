using LogMQ.Core;
using LogMQ.Services.Shared.LogManager;
using LogMQ.Storage.Contracts;
using System.Collections.Concurrent;
using System.Linq;

namespace LogMQ.Services.Presentation.API.Services;

public class FakeLogGeneratorService : ILogStorage
{
	private readonly ConcurrentLogMessageList messages = new();

	public FakeLogGeneratorService()
	{
		Task.Run(FillMessageQueue);
	}

	private async Task FillMessageQueue()
	{
		for (long count = 0; count < long.MaxValue; count++)
		{
			var message = new LogMessage { Guid = Guid.NewGuid(), LogLevel = Core.LogLevel.Information, Timestamp = DateTimeOffset.Now, Message = $"Message #{count}", Application = new() { Name = "Application1" } };
			messages.Add(message);
			Console.WriteLine($"Generated: {message.Message}");
			await Task.Delay(1000);
		}
	}

	public Task<List<LogMessage>> GetLogsByFilterAsync(LogFilter filter)
	{
		DateTimeOffset timeFrom = new(filter.TimeFrom, filter.TimeOffset);
		DateTimeOffset timeTo = new(filter.TimeTo, filter.TimeOffset);
		var messageList = messages.ToList();
		var logs = messageList.Where(x => x.Application.Name == filter.ApplicationName && x.Timestamp >= timeFrom && x.Timestamp <= timeTo);
		//logs = logs.OrderBy(x => x.Timestamp);
		return Task.FromResult(logs.ToList());
	}

	public Task<List<LogMessage>> GetLastLogsAsync(LastLogsFilter filter)
	{
		var messageList = messages.ToList();
		var startGuid = filter.StartFromId == Guid.Empty ? messageList[^2].Guid : filter.StartFromId;
		var startIndex = messageList.FindIndex(log => log.Guid == startGuid);
		var logs = startIndex == -1
			? []
			: messageList
				.Skip(startIndex + 1)
				.ToList();
		return Task.FromResult(logs);
	}

	public Task WriteLogMessageAsync(LogMessage logMessage)
	{
		throw new NotImplementedException();
	}
}

public class ConcurrentLogMessageList
{
	private List<LogMessage> internalList = [];
	private readonly ReaderWriterLockSlim lockSlim = new();

	public void Add(LogMessage item)
	{
		lockSlim.EnterWriteLock();
		try
		{
			internalList.Add(item);
			ReorderList();
		}
		finally
		{
			lockSlim.ExitWriteLock();
		}
	}

	public bool Remove(LogMessage item)
	{
		lockSlim.EnterWriteLock();
		try
		{
			var removed = internalList.Remove(item);
			ReorderList();
			return removed;
		}
		finally
		{
			lockSlim.ExitWriteLock();
		}
	}

	public LogMessage this[int index]
	{
		get
		{
			lockSlim.EnterReadLock();
			try
			{
				return internalList[index];
			}
			finally
			{
				lockSlim.ExitReadLock();
			}
		}
	}

	public int Count
	{
		get
		{
			lockSlim.EnterReadLock();
			try
			{
				return internalList.Count;
			}
			finally
			{
				lockSlim.ExitReadLock();
			}
		}
	}

	public void Clear()
	{
		lockSlim.EnterWriteLock();
		try
		{
			internalList.Clear();
		}
		finally
		{
			lockSlim.ExitWriteLock();
		}
	}
	public List<LogMessage> ToList()
	{
		lockSlim.EnterWriteLock();
		try
		{
			return internalList;
		}
		finally
		{
			lockSlim.ExitWriteLock();
		}
	}

	private void ReorderList() => internalList = [.. internalList.OrderBy(x => x.Timestamp)];
}