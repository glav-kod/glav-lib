using System.Globalization;
using FluentAssertions;
using GlavLib.Basics.DataTypes;

namespace GlavLib.Tests.Basics;

public sealed class UtcDateTimeTests
{
    //@formatter:off
    public static readonly TheoryData<string, string, DateTime> ParseUtcAtTimeZoneTestCases = new()
    {
        //value                                time zone                    expectation
        {"2026-03-05T10:00:00.079665Z",        "UTC",                       new DateTime(2026, 03, 05, 10, 0, 0, 79, 665)},
        {"2026-03-05T10:00:00.079665",         "UTC",                       new DateTime(2026, 03, 05, 10, 0, 0, 79, 665)},
        {"2026-03-05 10:08:05.079665",         "UTC",                       new DateTime(2026, 03, 05, 10, 8, 5, 79, 665)},

        {"2026-03-05 16:08:05.079665",         "Asia/Bishkek",              new DateTime(2026, 03, 05, 10, 8, 5, 79, 665)},
        {"2026-03-05 16:08:05.079665+06:00",   "Asia/Bishkek",              new DateTime(2026, 03, 05, 10, 8, 5, 79, 665)},
        
        {"2026-03-05 16:08:05.079665",         "Qyzylorda Standard Time",   new DateTime(2026, 03, 05, 11, 8, 5, 79, 665)},
        {"2026-03-05 16:08:05.079665+05:00",   "Qyzylorda Standard Time",   new DateTime(2026, 03, 05, 11, 8, 5, 79, 665)},

        {"2026-03-05 16:08:05.079665-05:00",   "Cuba Standard Time",        new DateTime(2026, 03, 05, 21, 8, 5, 79, 665)},
    };
    //@formatter:on

    [Theory]
    [MemberData(nameof(ParseUtcAtTimeZoneTestCases))]
    public void From_string_with_time_zone_tests(
            string value,
            string timeZoneId,
            DateTime expectedDateTime
        )
    {
        // arrange
        var timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);

        // act
        var utcDateTime = UtcDateTime.FromString(
                value: value,
                timeZone: timeZone
            );

        // assert
        utcDateTime.Value.Should().Be(expectedDateTime);
    }

    [Fact]
    private void It_should_create_from_utc_date_time()
    {
        var dateTime = Parse("2024-02-21 00:00:00Z");
        var result   = UtcDateTime.FromDateTime(dateTime);

        result.Value.Should().Be(
                new DateTime(
                        2024,
                        2,
                        21,
                        0,
                        0,
                        0,
                        DateTimeKind.Utc
                    )
            );
    }

    [Fact]
    private void It_should_create_from_local_date_time()
    {
        var dateTime = Parse("2024-02-21 06:00:00+06:00");
        var result   = UtcDateTime.FromDateTime(dateTime);

        result.Value.Should().Be(
                new DateTime(
                        2024,
                        2,
                        21,
                        0,
                        0,
                        0,
                        DateTimeKind.Utc
                    )
            );
    }

    [Fact]
    private void It_should_create_from_specified_time_zone()
    {
        var dateTime = Parse("2024-02-21 03:00:00");
        var tz       = TimeZoneInfo.FindSystemTimeZoneById("Europe/Moscow");

        var result = UtcDateTime.FromDateTime(dateTime, tz);

        result.Value.Should().Be(
                new DateTime(
                        2024,
                        2,
                        21,
                        0,
                        0,
                        0,
                        DateTimeKind.Utc
                    )
            );
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
