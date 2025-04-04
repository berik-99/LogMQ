using LogMQ.Core;
using ProtoBuf;

namespace LogMQ.Services.Shared.LogManager.Filters;

[ProtoContract]
public class ClearFilter
{
    [ProtoMember(1)]
    public string ApplicationName { get; set; }

    [ProtoMember(2)]
    public UniversalDateTime? OlderThan { get; set; }
}
