using ProtoBuf;

namespace LogMQ.Core;

[ProtoContract]
public class Wrapper<T>
{
    [ProtoMember(1)]
    public T Value { init; get; }

    public static implicit operator Wrapper<T>(T value)
    {
        return new Wrapper<T> { Value = value };
    }

    public static implicit operator T(Wrapper<T> protoValue)
    {
        return protoValue.Value;
    }
}