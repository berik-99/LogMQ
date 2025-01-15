using System.Text;
using LogMQ.Core;
using LogMQ.Storage.Contracts;
using Microsoft.Extensions.Logging;
using RocksDbSharp;

namespace LogMQ.Storage;

/// <summary>
/// Provides an implementation of <see cref="ILogStorage"/> using RocksDB as the underlying storage engine.
/// </summary>
public class RocksDbStorage : ILogStorage, IDisposable
{
    /// <summary>
    /// The instance of the RocksDB database.
    /// </summary>
    private readonly RocksDb db;

    /// <summary>
    /// A semaphore to synchronize access to the database.
    /// </summary>
    private readonly SemaphoreSlim semaphoreSlim = new(1, 1);

    /// <summary>
    /// The logger instance for logging internal events.
    /// </summary>
    private readonly ILogger<RocksDbStorage> logger;

    private readonly DbOptions dbOptions = new DbOptions()
            .SetCreateIfMissing(true)
            .SetCreateMissingColumnFamilies(true)
            .SetWriteBufferSize(64 * 1024 * 1024)
            .SetMaxWriteBufferNumber(3)
            .SetCompression(Compression.Snappy);

    private readonly string dbPath = Path.Combine(LogMQ.Services.Shared.Common.Defaults.DataFolder, "Data", "db");

    /// <summary>
    /// Initializes a new instance of the <see cref="RocksDbStorage"/> class.
    /// </summary>
    /// <param name="logger">An instance of <see cref="ILogger{TCategoryName}"/> for logging events.</param>
    public RocksDbStorage(ILogger<RocksDbStorage> logger)
    {
        this.logger = logger;
        logger.LogInformation("Init RocksDB Storage");
        Directory.CreateDirectory(dbPath);
        var families = GetColumnFamilies();
        db = RocksDb.Open(dbOptions, dbPath, families);
    }

    /// <inheritdoc />
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
            logger.LogInformation("Writed message for '{Application}': {Message}", logMessage.Application.Name, logMessage.Message);
        }
        finally
        {
            semaphoreSlim.Release();
        }
    }

    /// <inheritdoc />
    public async Task<List<LogMessage>> GetLogsAsync(LogFilter filter)
    {
        //TODO: Implement filters properly.
        await semaphoreSlim.WaitAsync();
        try
        {
            List<LogMessage> logMessages = [];

            if (db.TryGetColumnFamily(filter.ApplicationName, out var handle))
            {
                using var iterator = db.NewIterator(handle);
                iterator.SeekToFirst();

                while (iterator.Valid() && logMessages.Count < filter.Count)
                {
                    var keyBytes = iterator.Key();
                    var valueBytes = iterator.Value();

                    var (timestamp, _) = DeserializeKey(keyBytes);

                    var logMessage = LogMessage.Deserialize(valueBytes);
                    logMessages.Add(logMessage);

                    iterator.Next();
                }
                //logMessages = logMessages.OrderBy(msg => msg.Timestamp).Take(filter.Count).ToList();
            }
            else
            {
                logger.LogWarning("Column family {ColumnFamilyName} not found.", filter.ApplicationName);
            }
            return logMessages;
        }
        finally
        {
            semaphoreSlim.Release();
        }
    }

    /// <inheritdoc />
    public async Task<long> GetLogsCountAsync(string applicationName)
    {
        return await Task.Run(() =>
        {
            if (db.TryGetColumnFamily(applicationName, out var handle))
            {
                string propertyValue = db.GetProperty("rocksdb.estimate-num-keys", handle);
                return long.TryParse(propertyValue, out long numberOfRecords) ? numberOfRecords : 0;
            }
            else
            {
                logger.LogWarning("Column family {ColumnFamilyName} not found.", applicationName);
                return 0;
            }
        });
    }

    /// <summary>
    /// Serializes a unique key composed of a timestamp and a GUID for RocksDB storage.
    /// </summary>
    /// <param name="dateTimeOffset">The timestamp to include in the key.</param>
    /// <param name="guid">The GUID to include in the key.</param>
    /// <returns>A byte array representing the serialized key.</returns>
    public static byte[] SerializeKey(DateTimeOffset dateTimeOffset, Guid guid)
    {
        string dateTimeString = dateTimeOffset.ToString("yyyyMMddHHmmssffff");
        string guidString = guid.ToString("N");
        string combinedKey = $"{dateTimeString}-{guidString}";
        return Encoding.UTF8.GetBytes(combinedKey);
    }

    /// <summary>
    /// Deserializes a key into its timestamp and GUID components.
    /// </summary>
    /// <param name="keyBytes">The byte array representing the serialized key.</param>
    /// <returns>A tuple containing the deserialized <see cref="DateTimeOffset"/> and <see cref="Guid"/>.</returns>
    public static (DateTimeOffset, Guid) DeserializeKey(byte[] keyBytes)
    {
        string combinedKey = Encoding.UTF8.GetString(keyBytes);
        string[] parts = combinedKey.Split('-');
        string dateTimeString = parts[0];
        string guidString = parts[1];
        DateTimeOffset dateTimeOffset = DateTimeOffset.ParseExact(dateTimeString, "yyyyMMddHHmmssffff", null);
        Guid guid = Guid.Parse(guidString);

        return (dateTimeOffset, guid);
    }

    /// <summary>
    /// Retrieves the column families for a RocksDB instance.
    /// </summary>
    /// <returns>A <see cref="ColumnFamilies"/> collection representing the column families in the database.</returns>
    private ColumnFamilies GetColumnFamilies()
    {
        ColumnFamilies families = [];
        List<string> familiesStr = [];

        if (Directory.GetFiles(dbPath).Length > 0)
            familiesStr = RocksDb.ListColumnFamilies(dbOptions, dbPath).ToList();

        foreach (var family in familiesStr)
            families.Add(family, new());

        return families;
    }

    /// <summary>
    /// Disposes the resources used by the instance.
    /// </summary>
    public void Dispose()
    {
        db?.Dispose();
        semaphoreSlim?.Dispose();
        GC.SuppressFinalize(this);
    }
}