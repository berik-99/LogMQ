using ProtoBuf;

namespace LogMQ.Core;

[ProtoContract]
public class LogFilter
{
    [ProtoMember(1)]
    public string ApplicationName { get; set; }

    [ProtoMember(2)]
    public Guid ApplicationId { get; set; }

    [ProtoMember(3)]
    public DateTime TimeFrom { get; set; }

    [ProtoMember(4)]
    public DateTime TimeTo { get; set; }

    [ProtoMember(5)]
    public TimeSpan TimeOffset { get; set; }

    [ProtoMember(6)]
    public int Count { get; set; }
}