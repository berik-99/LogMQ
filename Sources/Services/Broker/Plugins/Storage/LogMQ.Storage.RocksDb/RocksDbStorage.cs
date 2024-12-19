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
    public async Task WriteLogMessage(LogMessage logMessage)
    {
        await semaphoreSlim.WaitAsync();
        try
        {
            bool columnFamilyExists = db.TryGetColumnFamily(logMessage.Application.Name, out ColumnFamilyHandle handle);
            if (!columnFamilyExists) handle = db.CreateColumnFamily(new ColumnFamilyOptions(), logMessage.Application.Name);

            byte[] key = SerializeKey(logMessage.Timestamp, logMessage.Guid);
            byte[] message = logMessage.Serialize();

            db.Put(key, message, handle);
            //logger.LogInformation("{Application} - {Method} - {Message}", logMessage.Application.Name, logMessage.Metadata.MethodName, logMessage.Message);
        }
        finally
        {
            semaphoreSlim.Release();
        }
    }

    /// <inheritdoc />
    public async Task<List<LogMessage>> GetLogMessages(DateTimeOffset start, DateTimeOffset end, int count)
    {
        await semaphoreSlim.WaitAsync();
        try
        {
            var logMessages = new List<LogMessage>();

            // Recupera tutte le column family dal database
            var columnFamilies = GetColumnFamilies();
            foreach (var columnFamily in columnFamilies)
            {
                var handle = db.GetColumnFamily(columnFamily.Name);
                if (handle == null)
                {
                    logger.LogWarning("Column family {ColumnFamilyName} not found.", columnFamily.Name);
                    continue;
                }

                using var iterator = db.NewIterator(handle);
                iterator.SeekToFirst();

                while (iterator.Valid() && logMessages.Count < count)
                {
                    var keyBytes = iterator.Key();
                    var valueBytes = iterator.Value();

                    var (timestamp, _) = DeserializeKey(keyBytes);

                    if (timestamp >= start && timestamp <= end)
                    {
                        var logMessage = LogMessage.Deserialize(new MemoryStream(valueBytes));
                        logMessages.Add(logMessage);
                    }

                    iterator.Next();
                }
            }

            return logMessages.OrderBy(msg => msg.Timestamp).Take(count).ToList();
        }
        finally
        {
            semaphoreSlim.Release();
        }
    }

    /// <summary>
    /// Serializes a unique key composed of a timestamp and a GUID for RocksDB storage.
    /// </summary>
    /// <param name="dateTimeOffset">The timestamp to include in the key.</param>
    /// <param name="guid">The GUID to include in the key.</param>
    /// <returns>A byte array representing the serialized key.</returns>
    private static byte[] SerializeKey(DateTimeOffset dateTimeOffset, Guid guid)
    {
        long unixTimestamp = dateTimeOffset.ToUnixTimeMilliseconds();
        short offsetMinutes = (short)dateTimeOffset.Offset.TotalMinutes;

        byte[] keyBytes = new byte[sizeof(long) + sizeof(short) + guid.ToByteArray().Length];
        BitConverter.GetBytes(unixTimestamp).CopyTo(keyBytes, 0);
        BitConverter.GetBytes(offsetMinutes).CopyTo(keyBytes, sizeof(long));
        guid.ToByteArray().CopyTo(keyBytes, sizeof(long) + sizeof(short));

        return keyBytes;
    }

    /// <summary>
    /// Deserializes a key into its timestamp and GUID components.
    /// </summary>
    /// <param name="keyBytes">The byte array representing the serialized key.</param>
    /// <returns>A tuple containing the deserialized <see cref="DateTimeOffset"/> and <see cref="Guid"/>.</returns>
    private static (DateTimeOffset dateTimeOffset, Guid guid) DeserializeKey(byte[] keyBytes)
    {
        long unixTimestamp = BitConverter.ToInt64(keyBytes, 0);
        short offsetMinutes = BitConverter.ToInt16(keyBytes, sizeof(long));
        TimeSpan offset = TimeSpan.FromMinutes(offsetMinutes);

        byte[] guidBytes = new byte[16];
        Array.Copy(keyBytes, sizeof(long) + sizeof(short), guidBytes, 0, 16);
        Guid guid = new(guidBytes);

        DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(unixTimestamp).ToOffset(offset);
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