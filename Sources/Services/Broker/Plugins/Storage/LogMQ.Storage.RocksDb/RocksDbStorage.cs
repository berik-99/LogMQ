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
    private const string timestampSerializationFormat = "yyyyMMddHHmmssffff";

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
        ColumnFamilies families = GetColumnFamilies();
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

            if (db.TryGetColumnFamily(filter.ApplicationName, out ColumnFamilyHandle handle))
            {
                using Iterator iterator = db.NewIterator(handle);
                iterator.SeekToFirst();

                while (iterator.Valid() && logMessages.Count < filter.Count)
                {
                    byte[] valueBytes = iterator.Value();
                    LogMessage logMessage = LogMessage.Deserialize(valueBytes);
                    if (logMessage.Timestamp >= filter.TimeFrom && logMessage.Timestamp <= filter.TimeTo)
                        logMessages.Add(logMessage);
                    iterator.Next();
                }
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
    public async Task<long> GetTotalLogsCountAsync(string applicationName)
    {
        return await Task.Run(() =>
        {
            if (db.TryGetColumnFamily(applicationName, out ColumnFamilyHandle handle))
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

    /// <inheritdoc />
    public async Task<List<string>> GetLogApplications()
    {
        return await Task.Run(() => GetColumnFamilies().ToList().ConvertAll(x => x.Name));
    }

    /// <summary>
    /// Serializes a unique key composed of a timestamp and a GUID for RocksDB storage.
    /// </summary>
    /// <param name="timestamp">The timestamp to include in the key.</param>
    /// <param name="guid">The GUID to include in the key.</param>
    /// <returns>A byte array representing the serialized key.</returns>
    public static byte[] SerializeKey(UniversalDateTime timestamp, Guid guid)
    {
        string dateTimeString = timestamp.ToDateTimeOffset().UtcDateTime.ToString(timestampSerializationFormat);
        string guidString = guid.ToString("N");
        string combinedKey = $"{dateTimeString}-{guidString}";
        return Encoding.UTF8.GetBytes(combinedKey);
    }

    /// <summary>
    /// Deserializes a key into its timestamp and GUID components.
    /// </summary>
    /// <param name="keyBytes">The byte array representing the serialized key.</param>
    /// <returns>A tuple containing the deserialized <see cref="DateTimeOffset"/> and <see cref="Guid"/>.</returns>
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

        foreach (string family in familiesStr)
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