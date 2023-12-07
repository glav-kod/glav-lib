using System.Xml;
using System.Xml.Serialization;
using JetBrains.Annotations;
using Microsoft.IO;

namespace GlavLib.App.Http;

internal static class XmlResultHelper
{
    public static readonly RecyclableMemoryStreamManager Instance = new();
    
    public static readonly XmlSerializerNamespaces EmptyNamespaces = new(new[]
    {
        new XmlQualifiedName(string.Empty, string.Empty)
    });
    
}

public class FromXml<T> : IQueryArg
{
    private static readonly XmlSerializer Serializer = new(typeof(T));

    public required T Value { get; init; }

    [PublicAPI]
    public static bool TryParse(string? value, out FromXml<T>? result)
    {
        if (value is null)
        {
            result = null;
            return false;
        }

        using var stringReader = new StringReader(value);
        using var xmlReader    = new XmlTextReader(stringReader);

        result = new FromXml<T>
        {
            Value = (T)Serializer.Deserialize(xmlReader)!
        };
        return true;
    }

    public object? GetArgumentValue() => Value;
}