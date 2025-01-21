//using LogMQ.Core;
//using LogMQ.Storage.Contracts;
//using Microsoft.Extensions.Logging;
//using RocksDbSharp;

//namespace LogMQ.Storage;

///// <summary>
///// Provides an implementation of <see cref="ILogStorageWriter"/> using RocksDB as the underlying storage engine.
///// </summary>
//public class RocksDbStorageReader : ILogStorageReader, IDisposable
//{
//    /// <summary>
//    /// The instance of the RocksDB database.
//    /// </summary>
//    private readonly RocksDb db;

//    /// <summary>
//    /// A semaphore to synchronize access to the database.
//    /// </summary>
//    private readonly SemaphoreSlim semaphoreSlim = new(1, 1);

//    /// <summary>
//    /// The logger instance for logging internal events.
//    /// </summary>
//    private readonly ILogger<ILogStorageReader> logger;

//    private readonly DbOptions dbOptions = new DbOptions()
//            .SetCreateIfMissing(true)
//            .SetCreateMissingColumnFamilies(true)
//            .SetWriteBufferSize(64 * 1024 * 1024)
//            .SetMaxWriteBufferNumber(3)
//            .SetCompression(Compression.Snappy);

//    private readonly string db0Path = Path.Combine(LogMQ.Services.Shared.Common.Defaults.DataFolder, "Data", "RocksDB", "db_0");
//    private readonly string db1Path = Path.Combine(LogMQ.Services.Shared.Common.Defaults.DataFolder, "Data", "RocksDB", "db_1");

//    /// <summary>
//    /// Initializes a new instance of the <see cref="RocksDbStorage"/> class.
//    /// </summary>
//    /// <param name="logger">An instance of <see cref="ILogger{TCategoryName}"/> for logging events.</param>
//    public RocksDbStorageReader(ILogger<RocksDbStorageReader> logger)
//    {
//        this.logger = logger;
//        logger.LogInformation("Init RocksDB Storage Reader");
//        ColumnFamilies families = GetColumnFamilies();
//        db = RocksDb.OpenAsSecondary(dbOptions, db0Path, db1Path, families);
//    }

//    /// <inheritdoc />
//    public async Task<List<LogMessage>> GetLogsAsync(LogFilter filter)
//    {
//        //TODO: Implement filters properly.
//        await semaphoreSlim.WaitAsync();
//        try
//        {
//            List<LogMessage> logMessages = [];

//            if (db.TryGetColumnFamily(filter.ApplicationName, out ColumnFamilyHandle handle))
//            {
//                db.TryCatchUpWithPrimary();
//                using Iterator iterator = db.NewIterator(handle);
//                iterator.SeekToFirst();

//                while (iterator.Valid() && logMessages.Count < filter.Count)
//                {
//                    byte[] valueBytes = iterator.Value();
//                    LogMessage logMessage = LogMessage.Deserialize(valueBytes);
//                    if (logMessage.Timestamp >= filter.DateFrom && logMessage.Timestamp <= filter.DateTo)
//                        logMessages.Add(logMessage);
//                    iterator.Next();
//                }
//            }
//            else
//            {
//                logger.LogWarning("Column family {ColumnFamilyName} not found.", filter.ApplicationName);
//            }
//            return logMessages;
//        }
//        finally
//        {
//            semaphoreSlim.Release();
//        }
//    }

//    /// <inheritdoc />
//    public async Task<Wrapper<long>> GetTotalLogsCountAsync(Wrapper<string> applicationName)
//    {
//        return await Task.Run(() =>
//        {
//            db.TryCatchUpWithPrimary();
//            if (db.TryGetColumnFamily(applicationName, out ColumnFamilyHandle handle))
//            {
//                string propertyValue = db.GetProperty("rocksdb.estimate-num-keys", handle);
//                return int.TryParse(propertyValue, out int numberOfRecords) ? numberOfRecords : 0;
//            }
//            else
//            {
//                logger.LogWarning("Column family {ColumnFamilyName} not found.", applicationName);
//                return 0;
//            }
//        });
//    }

//    /// <inheritdoc />
//    public async Task<List<string>> GetLogApplications()
//    {
//        return await Task.Run(() => GetColumnFamilies().ToList().ConvertAll(x => x.Name));
//    }

//    /// <summary>
//    /// Retrieves the column families for a RocksDB instance.
//    /// </summary>
//    /// <returns>A <see cref="ColumnFamilies"/> collection representing the column families in the database.</returns>
//    private ColumnFamilies GetColumnFamilies()
//    {
//        ColumnFamilies families = [];
//        List<string> familiesStr = [];

//        if (Directory.GetFiles(db0Path).Length > 0)
//            familiesStr = RocksDb.ListColumnFamilies(dbOptions, db0Path).ToList();

//        foreach (string family in familiesStr)
//            families.Add(family, new());

//        return families;
//    }

//    /// <summary>
//    /// Disposes the resources used by the instance.
//    /// </summary>
//    public void Dispose()
//    {
//        db?.Dispose();
//        semaphoreSlim?.Dispose();
//        GC.SuppressFinalize(this);
//    }
//}