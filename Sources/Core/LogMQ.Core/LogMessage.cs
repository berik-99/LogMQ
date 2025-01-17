using ProtoBuf;

namespace LogMQ.Core;

/// <summary>
/// Represents a log message with metadata and information about the event.
/// </summary>
[ProtoContract]
public class LogMessage : IEquatable<LogMessage>
{
    /// <summary>
    /// Gets or sets the unique id of the log message.
    /// </summary>
    [ProtoMember(1)]
    public Guid Guid { get; set; }

    /// <summary>
    /// Gets or sets the UTC timestamp of when the log event occurred.
    /// </summary>
    [ProtoMember(2)]
    public UniversalDateTime Timestamp { get; set; }

    /// <summary>
    /// Gets or sets the severity level of the log message.
    /// </summary>
    [ProtoMember(3)]
    public LogLevel LogLevel { get; set; }

    /// <summary>
    /// Gets or sets the actual log message describing the event or action.
    /// </summary>
    [ProtoMember(4)]
    public string Message { get; set; }

    /// <summary>
    /// Gets or sets the application generating the log message.
    /// </summary>
    [ProtoMember(5)]
    public LogApplication Application { get; set; }

    /// <summary>
    /// Gets or sets additional metadata related to the log event, such as file, class, and method information.
    /// </summary>
    [ProtoMember(6)]
    public LogMetadata Metadata { get; set; }

    /// <summary>
    /// Gets or sets the exception message, if an exception was thrown during the log event.
    /// </summary>
    [ProtoMember(7)]
    public string ExceptionMessage { get; set; }

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
    /// <param name="message">The byte array containing the log message previously serialized.</param>
    /// <returns>The deserialized log message.</returns>
    public static LogMessage Deserialize(byte[] message)
    {
        ReadOnlySpan<byte> bytes = new(message);
        return Serializer.Deserialize<LogMessage>(bytes);
    }

    /// <inheritdoc/>
    public override bool Equals(object obj) => Equals(obj as LogMessage);

    /// <inheritdoc/>
    public bool Equals(LogMessage other) => other is not null && Guid.Equals(other.Guid);

    /// <inheritdoc/>
    public static bool operator ==(LogMessage left, LogMessage right) => EqualityComparer<LogMessage>.Default.Equals(left, right);

    /// <inheritdoc/>
    public static bool operator !=(LogMessage left, LogMessage right) => !(left == right);

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(Guid);
}