using FluentAssertions;
using GlavLib.Basics.DataTypes;

namespace GlavLib.Tests.Basics;

public sealed class OptionalTests
{
    [Fact]
    public void It_should_set_value_if_defined()
    {
        Optional<long> oValue = 10;

        var result = oValue.GetValue(1);

        result.Should().Be(10);
    }

    [Fact]
    public void It_should_set_null_if_defined()
    {
        Optional<long?> oValue = null;

        var result = oValue.GetValue(1);

        result.Should().Be(null);
    }

    [Fact]
    public void It_should_not_leave_value_if_undefined()
    {
        var oValue = Optional<long?>.Undefined;

        var result = oValue.GetValue(1);

        result.Should().Be(1);
    }

    [Fact]
    public void It_should_not_leave_null_if_undefined()
    {
        var oValue = Optional<long?>.Undefined;

        var result = oValue.GetValue(null);

        result.Should().Be(1);
    }
}