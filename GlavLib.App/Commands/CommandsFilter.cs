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
                {
                    var errorResponse = CreateErrorResponse(
                            httpContext: httpContext,
                            error: commandResult.Error,
                            parameterName: commandResult.ParameterName,
                            parameterError: commandResult.ParameterError,
                            debugMessage: commandResult.XDebug
                        );

                    httpContext.Response.Headers.SetXStatus(XStatus.Error);
                    return TypedResults.Json(errorResponse);
                }

                httpContext.Response.Headers.SetXStatus(XStatus.OK);

                result = commandResult.Value;
            }

            if (result is CommandUnitResult commandUnitResult)
            {
                if (commandUnitResult.IsFailure)
                {
                    var errorResponse = CreateErrorResponse(
                            httpContext: httpContext,
                            error: commandUnitResult.Error,
                            parameterName: commandUnitResult.ParameterName,
                            parameterError: commandUnitResult.ParameterError,
                            debugMessage: commandUnitResult.XDebug
                        );

                    httpContext.Response.Headers.SetXStatus(XStatus.Error);
                    return TypedResults.Json(errorResponse);
                }

                httpContext.Response.Headers.SetXStatus(XStatus.OK);

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

    private static ErrorResponse CreateErrorResponse(
            HttpContext httpContext,
            Error error,
            string? parameterName,
            Error? parameterError,
            string? debugMessage
        )
    {
        var responseHeaders = httpContext.Response.Headers;

        if (debugMessage is not null)
            responseHeaders.SetXDebug(debugMessage);

        var serviceProvider = httpContext.RequestServices;
        var langContext     = serviceProvider.GetService<LanguageContext>();

        var localizedError = LocalizedError.Create(httpContext, langContext, error);

        if (parameterName is null || parameterError is null)
            return ErrorResponse.Create(localizedError, parameterErrors: null);

        var localizedParameterError = LocalizedError.Create(httpContext, langContext, parameterError.Value);

        var parameterErrors = new Dictionary<string, LocalizedError>
        {
            { parameterName, localizedParameterError }
        };

        return ErrorResponse.Create(localizedError, parameterErrors);
    }
}