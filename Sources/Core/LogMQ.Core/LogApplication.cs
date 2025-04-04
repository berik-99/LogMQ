using ProtoBuf;

namespace LogMQ.Core;

/// <summary>
/// Contains data related to an application
/// </summary>
[ProtoContract]
public class LogApplication : ProtoSerializable<LogApplication>
{
    /// <summary>
    /// Gets or sets the name of the application generating the log message.
    /// </summary>
    [ProtoMember(1)]
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the name of the machine where tha application is running.
    /// </summary>
    [ProtoMember(2)]
    public string Machine { get; set; }

    /// <summary>
    /// Gets or sets the ProcessID of the application generating the log message.
    /// </summary>
    [ProtoMember(3)]
    public int Pid { get; set; }

    /// <summary>
    /// Gets or sets the Category of the application generating the log message.
    /// </summary>
    [ProtoMember(4)]
    public string Category { get; set; }
}
