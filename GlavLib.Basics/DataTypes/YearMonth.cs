using System.Globalization;
using GlavLib.Abstractions.Results;
using GlavLib.Abstractions.Validation;
using GlavLib.Errors;

namespace GlavLib.Basics.DataTypes;

public sealed class YearMonth : SingleValueObject<DateTime>, IComparable<YearMonth>
{
    private const string Format = "yyyy-MM";

    public YearMonth(int year, int month)
        : base(new DateTime(year, month, 1))
    {
    }

    private YearMonth(DateTime value) : base(value)
    {
    }

    public Date StartOfTheMonth()
    {
        return Date.FromDateTime(Value);
    }

    public YearMonth NextMonth()
    {
        var newValue = Value.AddMonths(1);
        return new YearMonth(newValue);
    }

    public int CompareTo(YearMonth? other)
    {
        return other is null
            ? 1
            : DateTime.Compare(Value, other.Value);
    }

    public override string ToString()
    {
        return Value.ToString(Format);
    }

    public static bool operator <=(YearMonth value, YearMonth otherValue)
    {
        return value.Value <= otherValue.Value;
    }

    public static bool operator >=(YearMonth value, YearMonth otherValue)
    {
        return value.Value >= otherValue.Value;
    }

    public static bool operator <(YearMonth value, YearMonth otherValue)
    {
        return value.Value < otherValue.Value;
    }

    public static bool operator >(YearMonth value, YearMonth otherValue)
    {
        return value.Value > otherValue.Value;
    }

    public static Result<YearMonth, Error> FromString(string value)
    {
        if (!DateTime.TryParseExact(value, Format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateTime))
            return BasicErrors.WrongFormat;

        return new YearMonth(dateTime);
    }

    public static YearMonth FromDateTime(DateTime dateTime)
    {
        if (dateTime.Day != 1 || dateTime.Hour != 0 || dateTime.Minute != 0 || dateTime.Second != 0 || dateTime.Millisecond != 0)
            throw new InvalidOperationException($"Дата должна быть 1ое число месяца 00:00:00. Значение: {dateTime:yyyy.MM.dd HH:mm:ss fffffff}");

        return new YearMonth(dateTime.Date);
    }
}