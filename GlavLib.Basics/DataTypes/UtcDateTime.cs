using System.Globalization;
using GlavLib.Abstractions.Results;
using GlavLib.Abstractions.Validation;

namespace GlavLib.Basics.DataTypes;

public class UtcDateTime : SingleValueObject<DateTime>, IComparable<UtcDateTime>
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

    public static explicit operator UtcDateTime(string value)
    {
        var result = FromString(value);

        if (result.IsFailure)
            throw new InvalidOperationException(result.Error.Message);

        return result.Value;
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
        return x.Value > y.Value;
    }

    public static bool operator <=(UtcDateTime x, UtcDateTime y)
    {
        return x.Value >= y.Value;
    }

    public static UtcDateTime operator +(UtcDateTime utcDateTime, TimeSpan timeSpan)
    {
        return new UtcDateTime(utcDateTime.Value + timeSpan);
    }

    public static UtcDateTime operator -(UtcDateTime utcDateTime, TimeSpan timeSpan)
    {
        return new UtcDateTime(utcDateTime.Value - timeSpan);
    }

    public static Result<UtcDateTime, Error> FromString(string value)
    {
        if (!DateTime.TryParse(value,
                               CultureInfo.InvariantCulture,
                               DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeLocal,
                               out var dateTime))
        {
            return BasicErrors.WrongFormat;
        }

        return new UtcDateTime(dateTime);
    }

    public static UtcDateTime FromDateTime(DateTime dateTime)
    {
        if (dateTime.Kind == DateTimeKind.Unspecified)
            throw new InvalidOperationException(
                $"Wrong DateTime {dateTime:yyyy.MM.dd HH:mm:ss fffffff}, DateTime.Kind is unspecified");

        var universalTime = dateTime.ToUniversalTime();

        return new UtcDateTime(universalTime);
    }
}