using ProtoBuf;

namespace LogMQ.Messages;

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
	/// Gets or sets the application generating the log message.
	/// </summary>
	[ProtoMember(3)]
	public LogApplication Application { get; set; }

	/// <summary>
	/// Gets or sets additional metadata related to the log event, such as file, class, and method information.
	/// </summary>
	[ProtoMember(4)]
	public LogMetadata Meta { get; set; }

	[ProtoMember(5)]
	private DateTime timestampDateTime;

	[ProtoMember(6)]
	private TimeSpan timestampOffset;

	[ProtoMember(7)]
	public string TraceId { get; set; }

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