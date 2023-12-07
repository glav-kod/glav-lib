using FluentAssertions;
using FluentAssertions.Execution;
using GlavLib.SourceGenerators.Utils;

namespace GlavLib.SourceGenerators.Tests;

public sealed class ErrorMessageTemplateTests
{
    [Fact]
    public void It_should_parse_static_text()
    {
        var result = ErrorMessageTemplate.Parse("Неверный формат");

        using (new AssertionScope())
        {
            result.InterpolatedMessage.Should().Be("Неверный формат");
            result.Arguments.Should().BeEmpty();
        }
    }

    [Fact]
    public void It_should_parse_formatted_message()
    {
        var result = ErrorMessageTemplate.Parse("Слишком большая сумма: {sum:decimal}");

        using (new AssertionScope())
        {
            result.InterpolatedMessage.Should().Be("Слишком большая сумма: {sum}");
            result.Arguments.Should().BeEquivalentTo(new object[]
            {
                new ErrorArgument
                {
                    Name = "sum",
                    Type = "decimal",
                    FormatString = null,
                    IsOptional = false
                }
            });
        }
    }

    [Fact]
    public void It_should_parse_formatted_message_with_format()
    {
        var result = ErrorMessageTemplate.Parse("Слишком большая сумма: {sum:decimal:F2}");

        using (new AssertionScope())
        {
            result.InterpolatedMessage.Should().Be("Слишком большая сумма: {sum:F2}");
            result.Arguments.Should().BeEquivalentTo(new object[]
            {
                new ErrorArgument
                {
                    Name = "sum",
                    Type = "decimal",
                    FormatString = "F2",
                    IsOptional = false
                }
            });
        }
    }

    [Fact]
    public void It_should_parse_formatted_message_with_nullable_type()
    {
        var result = ErrorMessageTemplate.Parse("Слишком большая сумма: {sum:decimal?}");

        using (new AssertionScope())
        {
            result.InterpolatedMessage.Should().Be("Слишком большая сумма: {sum}");
            result.Arguments.Should().BeEquivalentTo(new object[]
            {
                new ErrorArgument
                {
                    Name = "sum",
                    Type = "decimal?",
                    FormatString = null,
                    IsOptional = true
                }
            });
        }
    }

    [Fact]
    public void It_should_parse_formatted_message_with_multiple_parameters()
    {
        var result = ErrorMessageTemplate.Parse("Слишком большая сумма: {sum:decimal}. Лимит: {limit:long?}");

        using (new AssertionScope())
        {
            result.InterpolatedMessage.Should().Be("Слишком большая сумма: {sum}. Лимит: {limit}");
            result.Arguments.Should().BeEquivalentTo(new object[]
            {
                new ErrorArgument
                {
                    Name = "sum",
                    Type = "decimal",
                    FormatString = null,
                    IsOptional = false
                },
                new ErrorArgument
                {
                    Name = "limit",
                    Type = "long?",
                    FormatString = null,
                    IsOptional = true
                }
            });
        }
    }

    [Theory]
    [InlineData("Слишком большая сумма: {sum:decimal")]
    [InlineData("Слишком большая сумма: {sum}")]
    public void ItShouldNotParseBadFormattedMessage(string template)
    {
        var result = ErrorMessageTemplate.Parse(template);

        using (new AssertionScope())
        {
            result.InterpolatedMessage.Should().Be(template);
            result.Arguments.Should().BeEmpty();
            result.Arguments.Should().BeEmpty();
        }
    }
}