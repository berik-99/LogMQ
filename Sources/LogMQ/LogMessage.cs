using ProtoBuf;

namespace LogMQ;

/// <summary>
/// Represents a log message with metadata and information about the event.
/// </summary>
[ProtoContract]
public class LogMessage
{
    /// <summary>
    /// Gets or sets the timestamp of when the log event occurred.
    /// </summary>
    public DateTimeOffset Timestamp
    {
        get => new(timestampDateTime, timestampOffset);
        set
        {
            timestampDateTime = value.DateTime;
            timestampOffset = value.Offset;
        }
    }

    /// <summary>
    /// Gets or sets the severity level of the log message.
    /// </summary>
    [ProtoMember(1)]
    public LogLevel LogLevel { get; set; }

    /// <summary>
    /// Gets or sets the actual log message describing the event or action.
    /// </summary>
    [ProtoMember(2)]
    public string Message { get; set; }

    /// <summary>
    /// Gets or sets the name of the application generating the log message.
    /// </summary>
    [ProtoMember(3)]
    public string Application { get; set; }

    /// <summary>
    /// Gets or sets the name of the machine where tha application is running.
    /// </summary>
    [ProtoMember(4)]
    public string Machine { get; set; }

    /// <summary>
    /// Gets or sets the ProcessID of the application generating the log message.
    /// </summary>
    [ProtoMember(5)]
    public int Pid { get; set; }

    /// <summary>
    /// Gets or sets the Category of the application generating the log message.
    /// </summary>
    [ProtoMember(6)]
    public string Category { get; set; }

    /// <summary>
    /// Gets or sets additional metadata related to the log event, such as file, class, and method information.
    /// </summary>
    [ProtoMember(7)]
    public LogMetadata Meta { get; set; }

    [ProtoMember(8)]
    private DateTime timestampDateTime;

    [ProtoMember(9)]
    private TimeSpan timestampOffset;

    /// <summary>
    /// Serializes the log message to the specified stream using Protocol Buffers (protobuf).
    /// </summary>
    /// <param name="stream">The stream to which the log message will be serialized.</param>
    /// <returns>The same stream that was passed in.</returns>
    public Stream SerializeToStream(Stream stream)
    {
        Serializer.Serialize(stream, this);
        return stream;
    }

    /// <summary>
    /// Serializes the log message and returns it as a byte array using Protocol Buffers (protobuf).
    /// </summary>
    /// <returns>A byte array containing the serialized log message.</returns>
    public byte[] Serialize()
    {
        using MemoryStream stream = new();
        SerializeToStream(stream);
        return stream.ToArray();
    }

    /// <summary>
    /// Deserializes a log message from the specified stream using Protocol Buffers (protobuf).
    /// </summary>
    /// <param name="stream">The stream from which the log message will be deserialized.</param>
    /// <returns>The deserialized log message.</returns>
    public static LogMessage Deserialize(Stream stream) => Serializer.Deserialize<LogMessage>(stream);
}

/// <summary>
/// Contains metadata related to a log message, such as the source file and method details.
/// </summary>
[ProtoContract]
public class LogMetadata
{
    /// <summary>
    /// Gets or sets the name of the source file where the log event occurred.
    /// </summary>
    [ProtoMember(1)]
    public string File { get; set; }

    /// <summary>
    /// Gets or sets the name of the class where the log event occurred.
    /// </summary>
    [ProtoMember(2)]
    public string Class { get; set; }

    /// <summary>
    /// Gets or sets the name of the method where the log event occurred.
    /// </summary>
    [ProtoMember(3)]
    public string MethodName { get; set; }

    /// <summary>
    /// Gets or sets the full signature of the method where the log event occurred.
    /// </summary>
    [ProtoMember(4)]
    public string MethodSignature { get; set; }

    /// <summary>
    /// Gets or sets the line number in the source file where the log event occurred.
    /// </summary>
    [ProtoMember(5)]
    public int Row { get; set; }

    /// <summary>
    /// Gets or sets the exception message, if any, that was thrown during the log event.
    /// </summary>
    [ProtoMember(6)]
    public string Exception { get; set; }
}
