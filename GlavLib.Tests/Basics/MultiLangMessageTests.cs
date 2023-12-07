using FluentAssertions;
using GlavLib.Basics.MultiLang;

namespace GlavLib.Tests.Basics;

public sealed class MultiLangMessageTests
{
    [Fact]
    public void It_should_format_static()
    {
        var multiLangMessage = new MultiLangMessage(new Dictionary<string, string>
        {
            ["ru"] = "Ошибка",
            ["en"] = "Error"
        });

        var result = multiLangMessage.Format(new[] { "en", "ru"}, args: null);

        result.Should().Be("Error");
    }
    
    [Fact]
    public void It_should_return_null_if_language_is_not_supported()
    {
        var multiLangMessage = new MultiLangMessage(new Dictionary<string, string>
        {
            ["ru"] = "Ошибка",
            ["en"] = "Error"
        });

        var result = multiLangMessage.Format(new[] { "ky" }, args: null);

        result.Should().BeNull();
    }
    
    [Fact]
    public void It_should_format_message()
    {
        var multiLangMessage = new MultiLangMessage(new Dictionary<string, string>
        {
            ["ru"] = "Ошибка: {message}"
        });

        var result = multiLangMessage.Format(new[] { "ru" }, args: new Dictionary<string, string>
        {
            ["message"] = "Все идет по плану (c)"
        });

        result.Should().Be("Ошибка: Все идет по плану (c)");
    }
    
    [Fact]
    public void It_should_ignore_argument_if_it_is_absent()
    {
        var multiLangMessage = new MultiLangMessage(new Dictionary<string, string>
        {
            ["ru"] = "Ошибка: {message}"
        });

        var result = multiLangMessage.Format(new[] { "ru" }, args: new Dictionary<string, string>());

        result.Should().Be("Ошибка: ");
    }
}