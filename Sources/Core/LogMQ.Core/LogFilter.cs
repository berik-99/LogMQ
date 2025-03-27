using ProtoBuf;

namespace LogMQ.Core;

[ProtoContract]
public class LogFilter
{
    [ProtoMember(1)]
    public string ApplicationName { get; set; }

    [ProtoMember(2)]
    public UniversalDateTime DateFrom { get; set; }

    [ProtoMember(3)]
    public UniversalDateTime DateTo { get; set; }

    [ProtoMember(4)]
    public long Count { get; set; }

    [ProtoMember(5)]
    public LogLevel? LogLevel { get; set; }
}
