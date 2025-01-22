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
    public int Count { get; set; }

    [ProtoMember(5)]
    public LogLevel? LogLevel { get; set; }
}

[ProtoContract]
public class Wrapper<T>
{
    [ProtoMember(1)]
    public T Value { get; set; }

    public static implicit operator Wrapper<T>(T value)
    {
        return new Wrapper<T> { Value = value };
    }

    public static implicit operator T(Wrapper<T> protoValue)
    {
        return protoValue.Value;
    }
}