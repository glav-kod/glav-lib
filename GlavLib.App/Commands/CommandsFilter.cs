using GlavLib.Abstractions.Validation;
using GlavLib.App.DomainEvents;
using GlavLib.App.Http;
using GlavLib.App.Validation;
using GlavLib.Basics.DomainEvents;
using GlavLib.Basics.MultiLang;
using GlavLib.Db;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GlavLib.App.Commands;

public static class CommandsFilterExtensions
{
    public static TBuilder UseCommands<TBuilder>(this TBuilder builder)
        where TBuilder : IEndpointConventionBuilder
    {
        return builder.AddEndpointFilterFactory(CommandsFilter.Factory);
    }
}

public sealed class CommandsFilter
{
    public static Func<EndpointFilterFactoryContext, EndpointFilterDelegate, EndpointFilterDelegate> Factory => (_, next) => Create(next);

    public static EndpointFilterDelegate Create(EndpointFilterDelegate next)
    {
        return async context =>
        {
            var serviceProvider = context.HttpContext.RequestServices;

            var logger              = serviceProvider.GetRequiredService<ILogger<CommandsFilter>>();
            var domainEventsHandler = serviceProvider.GetRequiredService<DomainEventsHandler>();

            using var domainEventsSession = DomainEventsSession.Bind();

            var result = await next(context);

            var httpContext = context.HttpContext;

            if (result is ICommandResult commandResult)
            {
                if (commandResult.IsFailure)
                    return ErrorResponse(httpContext, commandResult.ParameterName, commandResult.Error);

                httpContext.Response.Headers.AddOkStatus();

                result = commandResult.Value;
            }

            if (result is CommandUnitResult commandUnitResult)
            {
                if (commandUnitResult.IsFailure)
                    return ErrorResponse(httpContext, commandUnitResult.ParameterName, commandUnitResult.Error);

                httpContext.Response.Headers.AddOkStatus();

                result = Results.Ok();
            }

            if (domainEventsSession.Events.Count > 0)
            {
                using var dbTransaction = new DbTransaction();

                await domainEventsHandler.HandleAsync(domainEventsSession, context.HttpContext.RequestAborted);

                logger.LogInformation("Обработка ДоменныхСобытий завершилась успешно");

                dbTransaction.Commit();
            }


            return result;
        };
    }

    private static JsonHttpResult<ErrorResponse> ErrorResponse(
            HttpContext httpContext,
            string? parameterName,
            Error error
        )
    {
        httpContext.Response.Headers.AddErrorStatus();

        var acceptLanguageHeaders = httpContext.Request.Headers.AcceptLanguage;
        if (acceptLanguageHeaders.Count == 0)
            return ErrorResponse(parameterName, error.Message);

        var serviceProvider = httpContext.RequestServices;

        var langContext = serviceProvider.GetService<LanguageContext>();
        if (langContext is null)
            return ErrorResponse(parameterName, error.Message);

        var acceptLanguageHeader = acceptLanguageHeaders.FirstOrDefault(h => h is not null);
        if (acceptLanguageHeader is null)
            return ErrorResponse(parameterName, error.Message);

        var languages = AcceptLanguageHeaderHelper.Parse(acceptLanguageHeader);

        if (!langContext.Messages.TryGetValue(error.Key, out var message))
            return ErrorResponse(parameterName, error.Message);

        var localizedMessage = message.Format(languages, error.Args) ?? error.Message;

        return ErrorResponse(parameterName, localizedMessage);
    }

    private static JsonHttpResult<ErrorResponse> ErrorResponse(
            string? parameterName,
            string errorMessage
        )
    {
        if (parameterName is null)
        {
            return TypedResults.Json(new ErrorResponse
            {
                Message         = errorMessage,
                ParameterErrors = null
            });
        }

        return TypedResults.Json(new ErrorResponse
        {
            Message = null,
            ParameterErrors = new Dictionary<string, string>
            {
                [parameterName] = errorMessage
            }
        });
    }
}