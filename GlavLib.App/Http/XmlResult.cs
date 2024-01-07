using System.Xml.Serialization;
using Microsoft.AspNetCore.Http;

namespace GlavLib.App.Http;

public sealed class XmlResult<T> : IResult
{
    private static readonly XmlSerializer Serializer = new(typeof(T));

    private readonly T _result;

    public XmlResult(T result)
    {
        _result = result;
    }

    public async Task ExecuteAsync(HttpContext httpContext)
    {
        await using var ms = XmlResultHelper.Instance.GetStream();
        Serializer.Serialize(ms, _result, XmlResultHelper.EmptyNamespaces);

        httpContext.Response.ContentType = "application/xml";

        ms.Position = 0;
        await ms.CopyToAsync(httpContext.Response.Body);
    }
}