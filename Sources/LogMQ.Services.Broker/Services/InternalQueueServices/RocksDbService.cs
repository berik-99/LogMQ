namespace LogMQ.Broker.Services.InternalQueueServices;

using RocksDbSharp;
using System;
using System.Threading;
using System.Threading.Tasks;

public class RocksDbService : IDisposable
{
    private readonly ILogger _logger;
    private readonly RocksDb db;
    private readonly SemaphoreSlim semaphoreSlim = new(1, 1);

    public RocksDbService(ILogger<RocksDbService> logger)
    {
        _logger = logger;
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
            bool columnFamilyExists = db.TryGetColumnFamily(logMessage.Application, out ColumnFamilyHandle handle);
            if (!columnFamilyExists) handle = db.CreateColumnFamily(new ColumnFamilyOptions(), logMessage.Application);
            byte[] key = SerializeKey(logMessage.Timestamp, Guid.NewGuid());
            byte[] message = logMessage.Serialize();
            db.Put(key, message, handle);
            _logger.LogInformation("{application} - {message}", logMessage.Application, logMessage.Message);
        }
        finally
        {
            semaphoreSlim.Release();
        }
    }

    private static byte[] SerializeKey(DateTimeOffset dateTimeOffset, Guid guid)
    {
        // Ottieni il timestamp UNIX in millisecondi
        long unixTimestamp = dateTimeOffset.ToUnixTimeMilliseconds();

        // Ottieni l'offset in minuti
        short offsetMinutes = (short)dateTimeOffset.Offset.TotalMinutes;

        // Crea un array di byte per contenere il timestamp, l'offset e il GUID
        byte[] keyBytes = new byte[sizeof(long) + sizeof(short) + Guid.NewGuid().ToByteArray().Length];

        // Copia il timestamp nell'array di byte
        BitConverter.GetBytes(unixTimestamp).CopyTo(keyBytes, 0);

        // Copia l'offset nell'array di byte
        BitConverter.GetBytes(offsetMinutes).CopyTo(keyBytes, sizeof(long));

        // Copia il GUID nell'array di byte
        byte[] guidBytes = guid.ToByteArray();
        guidBytes.CopyTo(keyBytes, sizeof(long) + sizeof(short));

        return keyBytes;
    }

    private static (DateTimeOffset dateTimeOffset, Guid guid) DeserializeKey(byte[] keyBytes)
    {
        // Converti il byte[] indietro in un timestamp UNIX
        long unixTimestamp = BitConverter.ToInt64(keyBytes, 0);

        // Estrai l'offset
        short offsetMinutes = BitConverter.ToInt16(keyBytes, sizeof(long));
        TimeSpan offset = TimeSpan.FromMinutes(offsetMinutes);

        // Estrai il GUID
        byte[] guidBytes = new byte[16];
        Array.Copy(keyBytes, sizeof(long) + sizeof(short), guidBytes, 0, 16);
        Guid guid = new Guid(guidBytes);

        // Ricostruisci il DateTimeOffset
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
