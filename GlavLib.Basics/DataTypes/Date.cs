using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using GlavLib.Abstractions.Results;
using GlavLib.Abstractions.Validation;
using GlavLib.Errors;
using JetBrains.Annotations;

namespace GlavLib.Basics.DataTypes;

public class Date : SingleValueObject<DateTime>, IComparable<Date?>
{
    private const string Format = "yyyy-MM-dd";

    public Date(int year, int month, int day)
        : base(new DateTime(year, month, day))
    {
    }

    private Date(DateTime value) : base(value)
    {
    }

    public UtcDateTime UtcStartOfTheDay(TimeZoneInfo timeZone)
    {
        var dateTime = new DateTime(Value.Year, Value.Month, Value.Day);
        var result   = TimeZoneInfo.ConvertTime(dateTime, timeZone, TimeZoneInfo.Utc);

        return new UtcDateTime(result);
    }

    public UtcDateTime UtcStartOfTheMonth(TimeZoneInfo timezone)
    {
        var dateTime = new DateTime(Value.Year, Value.Month, 1);

        var result = TimeZoneInfo.ConvertTime(dateTime, timezone, TimeZoneInfo.Utc);

        return new UtcDateTime(result);
    }

    public Date AddDays(double value)
    {
        return new Date(Value.AddDays(value));
    }

    public Date AddMonths(int monthsNumber)
    {
        return new Date(Value.AddMonths(monthsNumber));
    }

    public Date AddYears(int yearsNumber)
    {
        return new Date(Value.AddYears(yearsNumber));
    }

    public int CompareTo(Date? other)
    {
        return other is null
            ? 1
            : DateTime.Compare(Value, other.Value);
    }

    public override string ToString()
    {
        return Value.ToString(Format);
    }

    public static explicit operator Date(string value)
    {
        var result = Create(value);

        if (result.IsFailure)
            throw new InvalidOperationException(result.Error.Message);

        return result.Value;
    }

    public static bool operator <=(Date value, Date otherValue)
    {
        return value.Value <= otherValue.Value;
    }

    public static bool operator >=(Date value, Date otherValue)
    {
        return value.Value >= otherValue.Value;
    }

    public static bool operator <(Date value, Date otherValue)
    {
        return value.Value < otherValue.Value;
    }

    public static bool operator >(Date value, Date otherValue)
    {
        return value.Value > otherValue.Value;
    }

    [PublicAPI]
    public static Date FromDateTime(DateTime dateTime)
    {
        if (dateTime.Hour != 0 || dateTime.Minute != 0 || dateTime.Second != 0 || dateTime.Millisecond != 0)
            throw new InvalidOperationException($"Date must not contain time. DateTime: {dateTime:yyyy.MM.dd HH:mm:ss fffffff}");

        return new Date(dateTime.Date);
    }

    [PublicAPI]
    public static Date FromString(string value)
    {
        return TryParse(value, out var result)
            ? result
            : throw new InvalidOperationException($"Wrong value format: {value}");
    }
    
    [PublicAPI]
    public static bool TryParse(string value, [MaybeNullWhen(false)] out Date result)
    {
        if (!DateTime.TryParseExact(s: value,
                                    format: Format, 
                                    provider: CultureInfo.InvariantCulture, 
                                    style: DateTimeStyles.None, 
                                    result: out var dateTime))
        {
            result = null;
            return false;
        }

        result = new Date(dateTime);
        return true;
    }

    [PublicAPI]
    public static Result<Date, Error> Create(string value)
    {
        if (!DateTime.TryParseExact(value, Format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateTime))
        {
            return BasicErrors.WrongFormat;
        }

        return new Date(dateTime);
    }
}