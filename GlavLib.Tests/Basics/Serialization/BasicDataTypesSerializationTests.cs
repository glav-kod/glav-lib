using FluentAssertions;
using FluentAssertions.Execution;
using GlavLib.Basics.DataTypes;
using GlavLib.Basics.Serialization;

namespace GlavLib.Tests.Basics.Serialization;

public sealed class BasicDataTypesSerializationTests
{
    [Fact]
    public void It_should_serialize_date()
    {
        var value = new Date(2025, 11, 15);

        var json   = GlavJsonSerializer.Serialize(value);
        var result = GlavJsonSerializer.Deserialize<Date>(json);

        using (new AssertionScope())
        {
            json.Should().Be("\"2025-11-15\"");
            result.Should().Be(value);
        }
    }

    [Fact]
    public void It_should_serialize_year_month()
    {
        var value = new YearMonth(2025, 11);

        var json   = GlavJsonSerializer.Serialize(value);
        var result = GlavJsonSerializer.Deserialize<YearMonth>(json);

        using (new AssertionScope())
        {
            json.Should().Be("\"2025-11\"");
            result.Should().Be(value);
        }
    }
}