using LogMQ.Core;
using LogLevel = LogMQ.Core.LogLevel;

namespace LogMQ.Services.Broker.Worker.Services.Models;

// This class is used to deserialize the log message from the database
public class QueryLogMessage
{
    public Guid Guid { get; set; }
    public DateTimeOffset Timestamp { get; set; }
    public LogLevel LogLevel { get; set; }
    public string Message { get; set; }
    public string ExceptionMessage { get; set; }
    public UnmanagedMemoryStream Metadata { init => metadata = ToArray(value); }
    public UnmanagedMemoryStream Application { init => application = ToArray(value); }

    private byte[] metadata;
    private byte[] application;

    private static byte[] ToArray(UnmanagedMemoryStream source)
    {
        //using MemoryStream tmpStream = new();
        //source.CopyTo(tmpStream);
        //return tmpStream.ToArray();
        using BinaryReader reader = new(source);
        return reader.ReadBytes((int)source.Length);
    }

    public LogMessage ToLogMessage()
    {
        return new LogMessage
        {
            Guid = Guid,
            LogLevel = LogLevel,
            Message = Message,
            Timestamp = Timestamp,
            Application = LogApplication.Deserialize(application),
            Metadata = LogMetadata.Deserialize(metadata),
            ExceptionMessage = ExceptionMessage
        };
    }
}