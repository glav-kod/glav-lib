using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace GlavLib.SourceGenerators.Tests.Helpers;

public sealed class TestAdditionalText : AdditionalText
{
    public override string Path { get; }

    private readonly string _text;

    public TestAdditionalText(string path, string text)
    {
        Path = path;
        _text = text;
    }

    public override SourceText GetText(CancellationToken cancellationToken = default)
    {
        return SourceText.From(_text, Encoding.UTF8);
    }
}