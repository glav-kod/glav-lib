using System.Net;
using GlavLib.Abstractions.DataTypes;
using Microsoft.AspNetCore.Http;

namespace GlavLib.App.Http;

[EnumObjectItem("OK", "OK", "OK")]
[EnumObjectItem("Error", "ERR", "Error")]
public sealed partial class XStatus : EnumObject;

public static class HttpCommandHeader
{
    public const string XStatus = "X-Status";
    public const string XDebug = "X-Debug";
}

public static class HttpCommandHeaderExtensions
{
    public static IHeaderDictionary SetXStatus(this IHeaderDictionary headerDictionary, XStatus status)
    {
        headerDictionary.Append(HttpCommandHeader.XStatus, status.Key);
        return headerDictionary;
    }

    public static IHeaderDictionary SetXDebug(this IHeaderDictionary headerDictionary, string debugMessage)
    {
        headerDictionary.Append(HttpCommandHeader.XDebug, WebUtility.UrlEncode(debugMessage));
        return headerDictionary;
    }
}