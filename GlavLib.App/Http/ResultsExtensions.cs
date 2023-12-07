using Microsoft.AspNetCore.Http;

namespace GlavLib.App.Http;

public static class ResultsExtensions
{
    public static IResult AsXml<T>(this T result)
    {
        return new XmlResult<T>(result);
    }
}