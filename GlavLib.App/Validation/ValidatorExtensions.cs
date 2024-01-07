using System.Diagnostics.CodeAnalysis;
using FluentValidation.Results;
using GlavLib.Basics.MultiLang;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace GlavLib.App.Validation;

public static class ValidatorExtensions
{
    public static ErrorResponse ToErrorResponse(this ValidationResult validationResult, HttpContext httpContext)
    {
        if (!TryLocaliseParameterErrors(httpContext, validationResult, out var parameterErrors))
        {
            parameterErrors = new Dictionary<string, string>();

            foreach (var error in validationResult.Errors)
                parameterErrors[error.PropertyName] = error.ErrorMessage;
        }


        return new ErrorResponse
        {
            Message         = null,
            ParameterErrors = parameterErrors
        };
    }

    private static bool TryLocaliseParameterErrors(
            HttpContext httpContext,
            ValidationResult validationResult,
            [MaybeNullWhen(false)] out Dictionary<string, string> parameterErrors
        )
    {
        var acceptLanguageHeaders = httpContext.Request.Headers.AcceptLanguage;
        if (acceptLanguageHeaders.Count == 0)
        {
            parameterErrors = null;
            return false;
        }


        var langContext = httpContext.RequestServices.GetService<LanguageContext>();
        if (langContext is null)
        {
            parameterErrors = null;
            return false;
        }

        var acceptLanguageHeader = acceptLanguageHeaders.FirstOrDefault(h => h is not null);
        if (acceptLanguageHeader is null)
        {
            parameterErrors = null;
            return false;
        }

        parameterErrors = new Dictionary<string, string>();

        var languages = AcceptLanguageHeaderHelper.Parse(acceptLanguageHeader);

        foreach (var error in validationResult.Errors)
        {
            if (!langContext.Messages.TryGetValue(error.ErrorCode, out var message))
            {
                parameterErrors[error.PropertyName] = error.ErrorMessage;
                continue;
            }

            var args = (IDictionary<string, string>)error.CustomState;

            var str = message.Format(languages, args);
            parameterErrors[error.PropertyName] = str ?? error.ErrorMessage;
        }

        return true;
    }
}