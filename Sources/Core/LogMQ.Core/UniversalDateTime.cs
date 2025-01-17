using ProtoBuf;

namespace LogMQ.Core;

[ProtoContract]
public readonly struct UniversalDateTime : IComparable, IComparable<UniversalDateTime>, IEquatable<UniversalDateTime>
{
    [ProtoMember(1)]
    private readonly DateTime timestamp;

    [ProtoMember(2)]
    private readonly TimeSpan utcOffset;

    public UniversalDateTime()
    {
        timestamp = DateTime.Now;
        utcOffset = TimeSpan.Zero;
    }

    public UniversalDateTime(DateTime dateTime)
    {
        timestamp = dateTime;
        utcOffset = dateTime.Kind == DateTimeKind.Utc ? TimeSpan.Zero : TimeZoneInfo.Local.GetUtcOffset(dateTime);
    }

    public UniversalDateTime(DateTime dateTime, TimeSpan offset)
    {
        timestamp = dateTime;
        utcOffset = offset;
    }

    public UniversalDateTime(DateTimeOffset dateTime)
    {
        timestamp = dateTime.DateTime;
        utcOffset = dateTime.Offset;
    }

    public DateTimeOffset ToDateTimeOffset() => new(timestamp, utcOffset);
    public DateTime ToDateTime() => ToDateTimeOffset().DateTime;
    public UniversalDateTime Add(TimeSpan value) => new(timestamp.Add(value), utcOffset);
    public UniversalDateTime AddDays(double days) => new(timestamp.AddDays(days), utcOffset);
    public UniversalDateTime AddHours(double hours) => new(timestamp.AddHours(hours), utcOffset);
    public UniversalDateTime AddMinutes(double minutes) => new(timestamp.AddMinutes(minutes), utcOffset);
    public UniversalDateTime AddSeconds(double seconds) => new(timestamp.AddSeconds(seconds), utcOffset);

    public static UniversalDateTime Now => new(DateTime.UtcNow, TimeSpan.Zero);
    public static UniversalDateTime UtcNow => new(DateTime.UtcNow, TimeSpan.Zero);
    public int Year => timestamp.Year;
    public int Month => timestamp.Month;
    public int Day => timestamp.Day;
    public int DayOfYear => timestamp.DayOfYear;
    public DayOfWeek DayOfWeek => timestamp.DayOfWeek;
    public int Hour => timestamp.Hour;
    public int Minute => timestamp.Minute;
    public int Second => timestamp.Second;
    public int Millisecond => timestamp.Millisecond;

    public override readonly string ToString() => new DateTimeOffset(timestamp, utcOffset).ToString();
    public readonly string ToString(string format) => new DateTimeOffset(timestamp, utcOffset).ToString(format);
    public readonly string ToString(string format, IFormatProvider formatProvider) => new DateTimeOffset(timestamp, utcOffset).ToString(format, formatProvider);

    public override bool Equals(object obj) => obj is UniversalDateTime other && Equals(other);
    public bool Equals(UniversalDateTime other) => timestamp.Equals(other.timestamp) && utcOffset.Equals(other.utcOffset);
    public override int GetHashCode() => HashCode.Combine(timestamp, utcOffset);

    public int CompareTo(object obj)
    {
        if (obj == null) return 1;
        if (obj is UniversalDateTime other) return CompareTo(other);
        throw new ArgumentException("Object must be of type UniversalDateTime");
    }

    public int CompareTo(UniversalDateTime other) => ToDateTimeOffset().CompareTo(other.ToDateTimeOffset());

    public static bool operator ==(UniversalDateTime left, UniversalDateTime right) => left.Equals(right);
    public static bool operator !=(UniversalDateTime left, UniversalDateTime right) => !(left == right);
    public static bool operator <(UniversalDateTime left, UniversalDateTime right) => left.CompareTo(right) < 0;
    public static bool operator <=(UniversalDateTime left, UniversalDateTime right) => left.CompareTo(right) <= 0;
    public static bool operator >(UniversalDateTime left, UniversalDateTime right) => left.CompareTo(right) > 0;
    public static bool operator >=(UniversalDateTime left, UniversalDateTime right) => left.CompareTo(right) >= 0;

    // Conversion operators
    public static implicit operator UniversalDateTime(DateTime dateTime) => new(dateTime);
    public static implicit operator UniversalDateTime(DateTimeOffset dateTimeOffset) => new(dateTimeOffset);
    public static implicit operator DateTimeOffset(UniversalDateTime universalDateTime) => universalDateTime.ToDateTimeOffset();
}