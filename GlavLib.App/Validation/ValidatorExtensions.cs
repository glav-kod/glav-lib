using FluentValidation.Results;
using GlavLib.Abstractions.Validation;
using GlavLib.App.Commands;
using GlavLib.Basics.MultiLang;
using GlavLib.Errors;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace GlavLib.App.Validation;

public static class ValidatorExtensions
{
    public static ErrorResponse ToErrorResponse(this ValidationResult validationResult, HttpContext httpContext)
    {
        var langContext = httpContext.RequestServices.GetService<LanguageContext>();

        var parameterErrors = new Dictionary<string, Error>();

        foreach (var validationFailure in validationResult.Errors)
        {
            if (validationFailure.CustomState is not Error parameterError)
                continue;

            parameterErrors[validationFailure.PropertyName] = parameterError;
        }

        var localizedError           = LocalizedError.Create(httpContext, langContext, BasicErrors.CheckFields);
        var localizedParameterErrors = LocalizedError.Create(httpContext, langContext, parameterErrors);

        return ErrorResponse.Create(localizedError, localizedParameterErrors);
    }
}