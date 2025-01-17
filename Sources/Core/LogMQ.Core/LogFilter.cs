using ProtoBuf;

namespace LogMQ.Core;

[ProtoContract]
public class LogFilter
{
    [ProtoMember(1)]
    public string ApplicationName { get; set; }

    [ProtoMember(2)]
    public UniversalDateTime TimeFrom { get; set; }

    [ProtoMember(3)]
    public UniversalDateTime TimeTo { get; set; }

    [ProtoMember(4)]
    public int Count { get; set; }
}