using System.Globalization;
using GlavLib.Abstractions.Results;
using GlavLib.Abstractions.Validation;

namespace GlavLib.Basics.DataTypes;

public class Date : SingleValueObject<DateTime>
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
        var result = TimeZoneInfo.ConvertTime(dateTime, timeZone, TimeZoneInfo.Utc);

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

    public override string ToString()
    {
        return Value.ToString(Format);
    }

    public static explicit operator Date(string value)
    {
        var result = FromString(value);

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

    public static Result<Date, Error> FromString(string value)
    {
        if (!DateTime.TryParseExact(value, Format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateTime))
        {
            return BasicErrors.WrongFormat;
        }

        return new Date(dateTime);
    }

    public static Date FromDateTime(DateTime dateTime)
    {
        if (dateTime.Hour != 0 || dateTime.Minute != 0 || dateTime.Second != 0 || dateTime.Millisecond != 0)
            throw new InvalidOperationException($"Date must not contain time. DateTime: {dateTime:yyyy.MM.dd HH:mm:ss fffffff}");

        return new Date(dateTime.Date);
    }
}