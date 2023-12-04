using FluentAssertions;
using FluentAssertions.Execution;
using GlavLib.SourceGenerators.Utils;

namespace GlavLib.SourceGenerators.Tests;

public sealed class ErrorMessageTemplateTests
{
    [Fact]
    public void ItShouldParseStaticText()
    {
        var result = ErrorMessageTemplate.Parse("Неверный формат");

        using (new AssertionScope())
        {
            result.InterpolatedMessage.Should().Be("Неверный формат");
            result.ArgumentNames.Should().BeEmpty();
            result.ParameterNames.Should().BeEmpty();
            result.ParameterTypes.Should().BeEmpty();
        }
    }

    [Fact]
    public void ItShouldParseFormattedMessage()
    {
        var result = ErrorMessageTemplate.Parse("Слишком большая сумма: {sum:decimal}");

        using (new AssertionScope())
        {
            result.InterpolatedMessage.Should().Be("Слишком большая сумма: {sum}");
            result.ArgumentNames.Should().BeEquivalentTo("sum");
            result.ParameterNames.Should().BeEquivalentTo("Sum");
            result.ParameterTypes.Should().BeEquivalentTo("decimal");
        }
    }

    [Fact]
    public void ItShouldParseFormattedMessageWithNullableType()
    {
        var result = ErrorMessageTemplate.Parse("Слишком большая сумма: {sum:decimal?}");

        using (new AssertionScope())
        {
            result.InterpolatedMessage.Should().Be("Слишком большая сумма: {sum}");
            result.ArgumentNames.Should().BeEquivalentTo("sum");
            result.ParameterNames.Should().BeEquivalentTo("Sum");
            result.ParameterTypes.Should().BeEquivalentTo("decimal?");
        }
    }
    
    [Fact]
    public void ItShouldParseFormattedMessageWithMultipleParameters()
    {
        var result = ErrorMessageTemplate.Parse("Слишком большая сумма: {sum:decimal}. Лимит: {limit:long?}");

        using (new AssertionScope())
        {
            result.InterpolatedMessage.Should().Be("Слишком большая сумма: {sum}. Лимит: {limit}");
            result.ArgumentNames.Should().BeEquivalentTo("sum", "limit");
            result.ParameterNames.Should().BeEquivalentTo("Sum", "Limit");
            result.ParameterTypes.Should().BeEquivalentTo("decimal", "long?");
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
            result.ArgumentNames.Should().BeEmpty();
            result.ParameterNames.Should().BeEmpty();
            result.ParameterTypes.Should().BeEmpty();
        }
    }
}