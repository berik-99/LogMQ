using ProtoBuf;

namespace LogMQ.Core;

/// <summary>
/// Represents metadata associated with a log message, providing contextual information
/// such as the file name, method details, and exception details.
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
    /// Gets or sets the exception message, if an exception was thrown during the log event.
    /// </summary>
    [ProtoMember(6)]
    public string Exception { get; set; }
}
