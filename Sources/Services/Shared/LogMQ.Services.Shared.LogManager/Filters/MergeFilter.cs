using ProtoBuf;

namespace LogMQ.Services.Shared.LogManager.Filters;

[ProtoContract]
public class MergeFilter
{
    [ProtoMember(1)]
    public string SourceApplicationName { get; set; }

    [ProtoMember(2)]
    public string TargetApplicationName { get; set; }
}
