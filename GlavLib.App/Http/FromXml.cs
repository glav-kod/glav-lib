using System.Xml;
using System.Xml.Serialization;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.IO;

namespace GlavLib.App.Http;

internal static class XmlResultHelper
{
    public static readonly RecyclableMemoryStreamManager Instance = new();

    public static readonly XmlSerializerNamespaces EmptyNamespaces = new([
        new XmlQualifiedName(string.Empty, string.Empty)
    ]);
}

public class FromXml<T> : IEndpointArg
{
    private static readonly XmlSerializer Serializer = new(typeof(T));

    public required T Value { get; init; }

    [PublicAPI]
    public static ValueTask<FromXml<T>?> BindAsync(HttpContext context)
    {
        using var stringReader = new StreamReader(context.Request.Body);

        using var xmlReader = new XmlTextReader(stringReader);

        var result = new FromXml<T>
        {
            Value = (T)Serializer.Deserialize(xmlReader)!
        };

        return ValueTask.FromResult<FromXml<T>?>(result);
    }

    public object? GetArgumentValue() => Value;
}