using FluentValidation.Results;
using GlavLib.App.Http;
using Microsoft.AspNetCore.Http;
using SharpGrip.FluentValidation.AutoValidation.Endpoints.Results;

namespace GlavLib.App.Validation;

public sealed class ErrorResponseFactory : IFluentValidationAutoValidationResultFactory
{
    public IResult CreateResult(EndpointFilterInvocationContext context, ValidationResult validationResult)
    {
        var errorResponse = validationResult.ToErrorResponse(context.HttpContext);

        context.HttpContext.Response.Headers.SetXStatus(XStatus.Error);

        return Results.Ok(errorResponse);
    }
}