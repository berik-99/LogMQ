using ProtoBuf;

namespace LogMQ.Core;

[ProtoContract]
public record Wrapper<T>([property: ProtoMember(1)] T Value)
{
    public static implicit operator Wrapper<T>(T value)
    {
        return new Wrapper<T>(value);
    }

    public static implicit operator T(Wrapper<T> protoValue)
    {
        return protoValue.Value;
    }
}