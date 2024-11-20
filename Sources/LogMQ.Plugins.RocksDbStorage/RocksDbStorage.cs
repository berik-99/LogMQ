using LogMQ.Messages;
using LogMQ.Plugins.Storage.Contracts;
using Microsoft.Extensions.Logging;
using RocksDbSharp;

namespace LogMQ.Plugins.Storage;

public class RocksDbStorage : ILogMQStorage, IDisposable
{
	private readonly RocksDb db;
	private readonly SemaphoreSlim semaphoreSlim = new(1, 1);
	private readonly ILogger<RocksDbStorage> logger;

	public RocksDbStorage(ILogger<RocksDbStorage> logger)
	{
		this.logger = logger;
		logger.LogInformation("Init RockDB Storage");
		var dbPath = Path.Combine(Directory.GetCurrentDirectory(), "data/logmq-db");
		Directory.CreateDirectory(dbPath);


		var options = new DbOptions()
			.SetCreateIfMissing(true)
			.SetCreateMissingColumnFamilies(true)
			.SetWriteBufferSize(64 * 1024 * 1024)
			.SetMaxWriteBufferNumber(3)
			.SetCompression(Compression.Snappy);

		var families = GetColumnFamilies(options, dbPath);
		db = RocksDb.Open(options, dbPath, families);
	}

	public async Task WriteLogMessage(LogMessage logMessage)
	{
		await semaphoreSlim.WaitAsync();
		try
		{
			bool columnFamilyExists = db.TryGetColumnFamily(logMessage.Application.Name, out ColumnFamilyHandle handle);
			if (!columnFamilyExists) handle = db.CreateColumnFamily(new ColumnFamilyOptions(), logMessage.Application.Name);
			byte[] key = SerializeKey(logMessage.Timestamp, Guid.NewGuid());
			byte[] message = logMessage.Serialize();
			db.Put(key, message, handle);
			logger.LogInformation("{application} - {message}", logMessage.Application.Name, logMessage.Message);
		}
		finally
		{
			semaphoreSlim.Release();
		}
	}

	private static byte[] SerializeKey(DateTimeOffset dateTimeOffset, Guid guid)
	{
		long unixTimestamp = dateTimeOffset.ToUnixTimeMilliseconds();
		short offsetMinutes = (short)dateTimeOffset.Offset.TotalMinutes;
		byte[] keyBytes = new byte[sizeof(long) + sizeof(short) + Guid.NewGuid().ToByteArray().Length];
		BitConverter.GetBytes(unixTimestamp).CopyTo(keyBytes, 0);
		BitConverter.GetBytes(offsetMinutes).CopyTo(keyBytes, sizeof(long));
		byte[] guidBytes = guid.ToByteArray();
		guidBytes.CopyTo(keyBytes, sizeof(long) + sizeof(short));
		return keyBytes;
	}

	private static (DateTimeOffset dateTimeOffset, Guid guid) DeserializeKey(byte[] keyBytes)
	{
		long unixTimestamp = BitConverter.ToInt64(keyBytes, 0);
		short offsetMinutes = BitConverter.ToInt16(keyBytes, sizeof(long));
		TimeSpan offset = TimeSpan.FromMinutes(offsetMinutes);
		byte[] guidBytes = new byte[16];
		Array.Copy(keyBytes, sizeof(long) + sizeof(short), guidBytes, 0, 16);
		Guid guid = new Guid(guidBytes);
		DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(unixTimestamp).ToOffset(offset);
		return (dateTimeOffset, guid);
	}

	private static ColumnFamilies GetColumnFamilies(DbOptions options, string dbPath)
	{
		ColumnFamilies families = [];
		List<string> familiesStr = [];
		if (Directory.GetFiles(dbPath).Length > 0)
			familiesStr = RocksDb.ListColumnFamilies(options, dbPath).ToList();
		foreach (var family in familiesStr)
			families.Add(family, new());
		return families;
	}

	public void Dispose()
	{
		db?.Dispose();
		semaphoreSlim?.Dispose();
		GC.SuppressFinalize(this);
	}
}
