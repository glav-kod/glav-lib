using FluentValidation;
using GlavLib.App.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using SharpGrip.FluentValidation.AutoValidation.Endpoints.Results;
using SharpGrip.FluentValidation.AutoValidation.Shared.Extensions;

namespace GlavLib.App.Validation;

public static class AutoValidationExtensions
{
    public static RouteHandlerBuilder AddAutoValidation(this RouteHandlerBuilder routeHandlerBuilder)
    {
        routeHandlerBuilder.AddEndpointFilter<AutoValidationEndpointFilter>();
        return routeHandlerBuilder;
    }

    public static RouteGroupBuilder AddAutoValidation(this RouteGroupBuilder routeGroupBuilder)
    {
        routeGroupBuilder.AddEndpointFilter<AutoValidationEndpointFilter>();
        return routeGroupBuilder;
    }
}

public sealed class AutoValidationEndpointFilter : IEndpointFilter
{
    private readonly IServiceProvider _serviceProvider;

    public AutoValidationEndpointFilter(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context,
                                                EndpointFilterDelegate next)
    {
        for (var i = 0; i < context.Arguments.Count; i++)
        {
            var instanceToValidate = context.Arguments[i];

            if (instanceToValidate is null)
                continue;

            if (instanceToValidate is IQueryArg validateValue)
            {
                instanceToValidate = validateValue.GetArgumentValue();

                if (instanceToValidate is null)
                    continue;
            }

            var instanceType = instanceToValidate.GetType();

            if (instanceType.IsCustomType() && _serviceProvider.GetValidator(instanceType) is IValidator validator)
            {
                var validationContext = new ValidationContext<object>(instanceToValidate);

                var validationResult = await validator.ValidateAsync(validationContext, context.HttpContext.RequestAborted);
                if (validationResult.IsValid)
                    continue;

                var resultFactory = _serviceProvider.GetService<IFluentValidationAutoValidationResultFactory>();
                if (resultFactory is null)
                    resultFactory = new FluentValidationAutoValidationDefaultResultFactory();

                return resultFactory.CreateResult(context, validationResult);
            }
        }

        return await next(context);
    }
}