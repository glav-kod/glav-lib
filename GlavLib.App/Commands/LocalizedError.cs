using GlavLib.Abstractions.Validation;
using GlavLib.App.Validation;
using GlavLib.Basics.MultiLang;
using Microsoft.AspNetCore.Http;

namespace GlavLib.App.Commands;

internal readonly struct LocalizedError(string? code, string message)
{
    public string? Code { get; } = code;

    public string Message { get; } = message;

    public static LocalizedError Create(
            HttpContext httpContext,
            LanguageContext? langContext,
            Error error
        )
    {
        var acceptLanguageHeaders = httpContext.Request.Headers.AcceptLanguage;
        if (acceptLanguageHeaders.Count == 0)
            return new LocalizedError(error.Code, error.Message);

        if (langContext is null)
            return new LocalizedError(error.Code, error.Message);

        var acceptLanguageHeader = acceptLanguageHeaders.FirstOrDefault(h => h is not null);
        if (acceptLanguageHeader is null)
            return new LocalizedError(error.Code, error.Message);

        var languages = AcceptLanguageHeaderHelper.Parse(acceptLanguageHeader);

        if (!langContext.Messages.TryGetValue(error.Key, out var message))
            return new LocalizedError(error.Code, error.Message);

        var localizedMessage = message.Format(languages, error.Args) ?? error.Message;

        return new LocalizedError(error.Code, localizedMessage);
    }

    public static Dictionary<string, LocalizedError> Create(
            HttpContext httpContext,
            LanguageContext? langContext,
            IDictionary<string, Error> errors
        )
    {
        var acceptLanguageHeaders = httpContext.Request.Headers.AcceptLanguage;
        var acceptLanguageHeader  = acceptLanguageHeaders.FirstOrDefault(h => h is not null);

        var result = new Dictionary<string, LocalizedError>();

        if (acceptLanguageHeaders.Count == 0 || langContext is null || acceptLanguageHeader is null)
        {
            foreach (var (key, error) in errors)
                result[key] = new LocalizedError(error.Code, error.Message);

            return result;
        }

        var languages = AcceptLanguageHeaderHelper.Parse(acceptLanguageHeader);

        foreach (var (key, error) in errors)
        {
            if (!langContext.Messages.TryGetValue(error.Key, out var message))
            {
                result[key] = new LocalizedError(error.Code, error.Message);
                continue;
            }

            var strMessage = message.Format(languages, error.Args) ?? error.Message;
            result[key] = new LocalizedError(error.Code, strMessage);
        }

        return result;
    }
}