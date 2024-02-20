using System.Globalization;
using FluentAssertions;
using GlavLib.Basics.DataTypes;

namespace GlavLib.Tests.Basics;

public sealed class UtcDateTimeTests
{
    [Fact]
    private void It_should_create_from_utc_date_time()
    {
        var dateTime = Parse("2024-02-21 00:00:00Z");
        var result   = UtcDateTime.FromDateTime(dateTime);

        result.Value.Should().Be(new DateTime(2024, 2, 21, 0, 0, 0, DateTimeKind.Utc));
    }

    [Fact]
    private void It_should_create_from_local_date_time()
    {
        var dateTime = Parse("2024-02-21 06:00:00+06:00");
        var result   = UtcDateTime.FromDateTime(dateTime);

        result.Value.Should().Be(new DateTime(2024, 2, 21, 0, 0, 0, DateTimeKind.Utc));
    }

    [Fact]
    private void It_should_create_from_specified_time_zone()
    {
        var dateTime = Parse("2024-02-21 03:00:00");
        var tz       = TimeZoneInfo.FindSystemTimeZoneById("Europe/Moscow");

        var result = UtcDateTime.FromDateTime(dateTime, tz);

        result.Value.Should().Be(new DateTime(2024, 2, 21, 0, 0, 0, DateTimeKind.Utc));
    }

    private static DateTime Parse(string dateTime)
    {
        return DateTime.ParseExact(
                dateTime,
                "yyyy-MM-dd HH:mm:ssK",
                CultureInfo.InvariantCulture
            );
    }
}