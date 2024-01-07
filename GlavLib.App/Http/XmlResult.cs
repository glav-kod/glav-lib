using System.Xml.Serialization;
using Microsoft.AspNetCore.Http;

namespace GlavLib.App.Http;

public sealed class XmlResult<T>(T result) : IResult
{
    private static readonly XmlSerializer Serializer = new(typeof(T));

    public async Task ExecuteAsync(HttpContext httpContext)
    {
        await using var ms = XmlResultHelper.Instance.GetStream();
        Serializer.Serialize(ms, result, XmlResultHelper.EmptyNamespaces);

        httpContext.Response.ContentType = "application/xml";

        ms.Position = 0;
        await ms.CopyToAsync(httpContext.Response.Body);
    }
}