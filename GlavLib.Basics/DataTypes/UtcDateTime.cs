using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using GlavLib.Abstractions.Results;
using GlavLib.Abstractions.Validation;
using GlavLib.Errors;
using JetBrains.Annotations;

namespace GlavLib.Basics.DataTypes;

public class UtcDateTime : SingleValueObject<DateTime>, IComparable<UtcDateTime?>
{
    private const string Format = "yyyy-MM-ddTHH:mm:ssZ";

    public UtcDateTime(
            int year,
            int month,
            int day,
            int hour,
            int minute,
            int second
        )
        : base(
                new DateTime(
                        year,
                        month,
                        day,
                        hour,
                        minute,
                        second,
                        DateTimeKind.Utc
                    )
            )
    {
    }

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

    [PublicAPI]
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

    [PublicAPI]
    public static UtcDateTime FromDateTime(DateTime dateTime, TimeZoneInfo timeZone)
    {
        var utcDateTime = dateTime.Kind switch
        {
            DateTimeKind.Utc => dateTime,
            DateTimeKind.Local => TimeZoneInfo.ConvertTimeToUtc(dateTime, TimeZoneInfo.Local),
            DateTimeKind.Unspecified => TimeZoneInfo.ConvertTimeToUtc(dateTime, timeZone),
            _ => throw new InvalidOperationException(
                    $"Cannot convert DateTime {dateTime:yyyy.MM.dd HH:mm:ss fffffff} to UtcDateTime, " +
                    $"not supported DateTime.Kind: {dateTime.Kind}"
                )
        };

        return new UtcDateTime(utcDateTime);
    }

    [PublicAPI]
    public static UtcDateTime FromString(string value)
    {
        return TryParse(value, out var result)
            ? result
            : throw new InvalidOperationException($"Wrong value format: {value}");
    }

    [PublicAPI]
    public static UtcDateTime FromString(
            string value,
            TimeZoneInfo timeZone
        )
    {
        return TryParse(value, timeZone, out var result)
            ? result
            : throw new InvalidOperationException($"Wrong value format: {value}");
    }

    [PublicAPI]
    public static bool TryParse(
            string value,
            TimeZoneInfo timeZone,
            [NotNullWhen(returnValue: true)] out UtcDateTime? utcDateTime
        )
    {
        var isParsed = DateTime.TryParse(
                s: value,
                provider: CultureInfo.InvariantCulture,
                result: out var dateTime
            );

        if (!isParsed)
        {
            utcDateTime = null;
            return false;
        }

        utcDateTime = FromDateTime(dateTime, timeZone);
        return true;
    }

    [PublicAPI]
    public static bool TryParse(string value, [MaybeNullWhen(false)] out UtcDateTime result)
    {
        if (!DateTime.TryParseExact(
                    s: value,
                    format: "yyyy-MM-ddTHH:mm:ssZ",
                    provider: CultureInfo.InvariantCulture,
                    style: DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeLocal,
                    result: out var dateTime
                ))
        {
            result = null;
            return false;
        }

        result = new UtcDateTime(dateTime);
        return true;
    }

    [PublicAPI]
    public static Result<UtcDateTime, Error> Create(string value)
    {
        if (!TryParse(value, out var result))
            return BasicErrors.WrongFormat;

        return result;
    }
}
