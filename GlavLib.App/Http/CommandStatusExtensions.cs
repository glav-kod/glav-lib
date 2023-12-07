using Microsoft.AspNetCore.Http;

namespace GlavLib.App.Http;

internal static class CommandStatusExtensions
{
    private const string CommandStatus = "X-Status";

    public static IHeaderDictionary AddOkStatus(this IHeaderDictionary headerDictionary)
    {
        headerDictionary.Append(CommandStatus, "OK");
        return headerDictionary;
    }
    
    public static IHeaderDictionary AddErrorStatus(this IHeaderDictionary headerDictionary)
    {
        headerDictionary.Append(CommandStatus, "ERR");
        return headerDictionary;
    }
}