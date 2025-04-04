using LogMQ.Core;
using ProtoBuf;

namespace LogMQ.Services.Shared.LogManager.Filters;

[ProtoContract]
public class SearchFilter
{
    [ProtoMember(1)]
    public string ApplicationName { get; set; }

    [ProtoMember(2)]
    public UniversalDateTime DateFrom { get; set; }

    [ProtoMember(3)]
    public UniversalDateTime DateTo { get; set; }

    [ProtoMember(4)]
    public ulong? Count { get; set; }

    [ProtoMember(5)]
    public LogLevel? LogLevel { get; set; }
}
