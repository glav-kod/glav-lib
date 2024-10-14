using System.Globalization;
using GlavLib.Abstractions.Results;
using GlavLib.Abstractions.Validation;
using GlavLib.Errors;
using JetBrains.Annotations;

namespace GlavLib.Basics.DataTypes;

public class UtcDateTime : SingleValueObject<DateTime>, IComparable<UtcDateTime?>
{
    private const string Format = "yyyy-MM-ddTHH:mm:ssZ";

    internal UtcDateTime(DateTime value)
        : base(value)
    {
    }

    public override string ToString()
    {
        return Value.ToString(Format);
    }

    [PublicAPI]
    public Date ToDate(TimeZoneInfo timeZone)
    {
        var tzNow = TimeZoneInfo.ConvertTimeFromUtc(Value, timeZone);
        return new Date(tzNow.Year, tzNow.Month, tzNow.Day);
    }

    public int CompareTo(UtcDateTime? other)
    {
        return other is null
            ? 1
            : DateTime.Compare(Value, other.Value);
    }

    public static implicit operator DateTime(UtcDateTime value)
    {
        return value.Value;
    }

    public static explicit operator string(UtcDateTime value)
    {
        return value.Value.ToString(Format);
    }

    public static bool operator >(UtcDateTime x, UtcDateTime y)
    {
        return x.Value > y.Value;
    }

    public static bool operator >=(UtcDateTime x, UtcDateTime y)
    {
        return x.Value >= y.Value;
    }

    public static bool operator <(UtcDateTime x, UtcDateTime y)
    {
        return x.Value < y.Value;
    }

    public static bool operator <=(UtcDateTime x, UtcDateTime y)
    {
        return x.Value <= y.Value;
    }

    public static UtcDateTime operator +(UtcDateTime utcDateTime, TimeSpan timeSpan)
    {
        return new UtcDateTime(utcDateTime.Value + timeSpan);
    }

    public static UtcDateTime operator -(UtcDateTime utcDateTime, TimeSpan timeSpan)
    {
        return new UtcDateTime(utcDateTime.Value - timeSpan);
    }

    public static UtcDateTime FromDateTime(DateTime dateTime)
    {
        if (dateTime.Kind == DateTimeKind.Unspecified)
        {
            throw new InvalidOperationException(
                    $"Cannot convert DateTime {dateTime:yyyy.MM.dd HH:mm:ss fffffff} to UtcDateTime, " +
                    "DateTime.Kind is not specified"
                );
        }

        var utcDateTime = dateTime.ToUniversalTime();

        return new UtcDateTime(utcDateTime);
    }

    public static UtcDateTime FromDateTime(DateTime dateTime, TimeZoneInfo timeZone)
    {
        if (dateTime.Kind != DateTimeKind.Unspecified)
        {
            throw new InvalidOperationException(
                    $"Cannot convert DateTime {dateTime:yyyy.MM.dd HH:mm:ss fffffff} to UtcDateTime, " +
                    $"DateTime.Kind is {dateTime.Kind} (must be unspecified)"
                );
        }

        var utcDateTime = TimeZoneInfo.ConvertTimeToUtc(dateTime, timeZone);
        return new UtcDateTime(utcDateTime);
    }

    public static Result<UtcDateTime, Error> Create(string value)
    {
        if (!DateTime.TryParseExact(s: value,
                                    format: "yyyy-MM-ddTHH:mm:ssZ",
                                    provider: CultureInfo.InvariantCulture,
                                    style: DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeLocal,
                                    result: out var dateTime))
        {
            return BasicErrors.WrongFormat;
        }

        return new UtcDateTime(dateTime);
    }
}