using System.Text;
using LogMQ.Core;
using LogMQ.Storage.Contracts;
using Microsoft.Extensions.Logging;
using RocksDbSharp;

namespace LogMQ.Storage;

public class RocksDbStorage : ILogStorage
{
    private readonly string dbPath = string.Empty;
    private const string timestampSerializationFormat = "yyyyMMddHHmmssffff";
    private readonly RocksDb db;
    private readonly SemaphoreSlim semaphoreSlim = new(1, 1);
    private readonly ILogger<RocksDbStorage> log;
    private readonly DbOptions dbOptions = new DbOptions()
            .SetCreateIfMissing(true)
            .SetCreateMissingColumnFamilies(true)
            .SetWriteBufferSize(64 * 1024 * 1024)
            .SetMaxWriteBufferNumber(3)
            .SetCompression(Compression.Snappy);

    public RocksDbStorage(ILogger<RocksDbStorage> logger, RocksDbStorageConfiguration config)
    {
        dbPath = config.DbPath;
        log = logger;
        logger.LogInformation("Init RocksDB Storage");
        Directory.CreateDirectory(dbPath);
        ColumnFamilies families = GetColumnFamilies();
        db = RocksDb.Open(dbOptions, dbPath, families);
    }

    public async Task WriteLogMessageAsync(LogMessage logMessage)
    {
        await semaphoreSlim.WaitAsync();
        try
        {
            bool columnFamilyExists = db.TryGetColumnFamily(logMessage.Application.Name, out ColumnFamilyHandle handle);
            if (!columnFamilyExists) handle = db.CreateColumnFamily(new ColumnFamilyOptions(), logMessage.Application.Name);

            byte[] key = SerializeKey(logMessage.Timestamp, logMessage.Guid);
            byte[] message = logMessage.Serialize();
            db.Put(key, message, handle);
            log.LogInformation("Writed message for '{Application}': {Message}", logMessage.Application.Name, logMessage.Message);
        }
        finally
        {
            semaphoreSlim.Release();
        }
    }

    public async Task<List<LogMessage>> GetLogsAsync(LogFilter filter)
    {
        await semaphoreSlim.WaitAsync();
        try
        {
            List<LogMessage> logMessages = [];

            if (db.TryGetColumnFamily(filter.ApplicationName, out ColumnFamilyHandle handle))
            {
                if (filter.Count == -1) filter.Count = int.MaxValue;
                using Iterator iterator = db.NewIterator(handle);
                for (iterator.SeekToLast(); iterator.Valid(); iterator.Prev())
                {
                    if (logMessages.Count < filter.Count)
                    {
                        byte[] valueBytes = iterator.Value();
                        LogMessage logMessage = LogMessage.Deserialize(valueBytes);
                        if (logMessage.Timestamp >= filter.DateFrom && logMessage.Timestamp <= filter.DateTo)
                            logMessages.Insert(0, logMessage);
                    }
                }
            }
            else
            {
                log.LogWarning("Column family {ColumnFamilyName} not found.", filter.ApplicationName);
            }
            return logMessages;
        }
        finally
        {
            semaphoreSlim.Release();
        }
    }
    public async Task<Wrapper<long>> GetTotalLogsCountAsync(Wrapper<string> applicationName) => await Task.Run(() =>
                                                                                                     {
                                                                                                         if (db.TryGetColumnFamily(applicationName, out ColumnFamilyHandle handle))
                                                                                                         {
                                                                                                             string propertyValue = db.GetProperty("rocksdb.estimate-num-keys", handle);
                                                                                                             return int.TryParse(propertyValue, out int numberOfRecords) ? numberOfRecords : 0;
                                                                                                         }
                                                                                                         else
                                                                                                         {
                                                                                                             log.LogWarning("Column family {ColumnFamilyName} not found.", applicationName);
                                                                                                             return 0;
                                                                                                         }
                                                                                                     });

    public async Task<List<string>> GetLogApplications() => await Task.Run(() => GetColumnFamilies().ToList().ConvertAll(x => x.Name));

    public static byte[] SerializeKey(UniversalDateTime timestamp, Guid guid)
    {
        string dateTimeString = timestamp.ToDateTimeOffset().UtcDateTime.ToString(timestampSerializationFormat);
        string guidString = guid.ToString("N");
        string combinedKey = $"{dateTimeString}-{guidString}";
        return Encoding.UTF8.GetBytes(combinedKey);
    }

    public static (UniversalDateTime, Guid) DeserializeKey(byte[] keyBytes)
    {
        string combinedKey = Encoding.UTF8.GetString(keyBytes);
        string[] parts = combinedKey.Split('-');
        string dateTimeString = parts[0];
        string guidString = parts[1];
        DateTimeOffset dateTimeOffset = DateTime.ParseExact(dateTimeString, timestampSerializationFormat, null);
        Guid guid = Guid.Parse(guidString);

        return (new UniversalDateTime(dateTimeOffset), guid);
    }

    private ColumnFamilies GetColumnFamilies()
    {
        ColumnFamilies families = [];
        List<string> familiesStr = [];

        if (Directory.GetFiles(dbPath).Length > 0)
            familiesStr = RocksDb.ListColumnFamilies(dbOptions, dbPath).ToList();

        foreach (string family in familiesStr)
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
