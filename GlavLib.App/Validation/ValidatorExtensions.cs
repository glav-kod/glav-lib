using System.Diagnostics.CodeAnalysis;
using FluentValidation.Results;
using GlavLib.Abstractions.Validation;
using GlavLib.Basics.MultiLang;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace GlavLib.App.Validation;

public static class ValidatorExtensions
{
    public static ErrorResponse ToErrorResponse(this ValidationResult validationResult, HttpContext httpContext)
    {
        if (!TryLocalizeParameterErrors(httpContext, validationResult, out var parameterMessages))
        {
            parameterMessages = new Dictionary<string, string>();

            foreach (var error in validationResult.Errors)
                parameterMessages[error.PropertyName] = error.ErrorMessage;
        }

        var errorResponse = new ErrorResponse
        {
            Message           = null,
            ParameterMessages = parameterMessages
        };

        var parameterCodes = new Dictionary<string, string>();

        foreach (var error in validationResult.Errors)
        {
            if (error.ErrorCode is not null)
                parameterCodes[error.PropertyName] = error.ErrorCode;
        }

        if (parameterCodes.Count > 0)
            errorResponse.ParameterCodes = parameterCodes;

        return errorResponse;
    }

    private static bool TryLocalizeParameterErrors(
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

        foreach (var validationFailure in validationResult.Errors)
        {
            var error = (Error)validationFailure.CustomState;

            if (!langContext.Messages.TryGetValue(error.Key, out var message))
            {
                parameterErrors[validationFailure.PropertyName] = validationFailure.ErrorMessage;
                continue;
            }

            var str = message.Format(languages, error.Args);
            parameterErrors[validationFailure.PropertyName] = str ?? validationFailure.ErrorMessage;
        }

        return true;
    }
}