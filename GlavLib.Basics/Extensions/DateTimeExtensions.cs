using JetBrains.Annotations;

namespace GlavLib.Basics.Extensions;

public enum DateTimePart
{
    Year,
    Month,
    Day,
    Hour,
    Minute
}

[PublicAPI]
public static class DateTimeExtensions
{
    public static DateTime Trunc(this DateTime dateTime, DateTimePart dateTimePart)
    {
        switch (dateTimePart)
        {
            case DateTimePart.Year:
                return new DateTime(dateTime.Year, 1, 1, 0, 0, 0, dateTime.Kind);

            case DateTimePart.Month:
                return new DateTime(dateTime.Year, dateTime.Month, 1, 0, 0, 0, dateTime.Kind);

            case DateTimePart.Day:
                return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, 0, 0, 0, dateTime.Kind);

            case DateTimePart.Hour:
                return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, 0, 0, dateTime.Kind);

            case DateTimePart.Minute:
                return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Day, dateTime.Second, 0, dateTime.Kind);

            default:
                throw new ArgumentOutOfRangeException(nameof(dateTimePart), dateTimePart, null);
        }
    }

    public static DateTime SetTime(this DateTime dateTime,
                                   int hour,
                                   int minute,
                                   int second)
    {
        return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, hour, minute, second, dateTime.Kind);
    }
    
    public static DateTime AssumeLocal(this DateTime dateTime)
    {
        if (dateTime.Kind == DateTimeKind.Unspecified)
        {
            return new DateTime(dateTime.Year, 
                                dateTime.Month, 
                                dateTime.Day, 
                                dateTime.Hour, 
                                dateTime.Minute, 
                                dateTime.Second,
                                DateTimeKind.Local);
        }

        return dateTime;
    }
}