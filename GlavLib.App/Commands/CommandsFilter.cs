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
                    return ErrorResponse(httpContext,
                                         commandResult.ParameterName,
                                         commandResult.Error,
                                         commandResult.XDebug);

                httpContext.Response.Headers.SetXStatus(XStatus.OK);

                result = commandResult.Value;
            }

            if (result is CommandUnitResult commandUnitResult)
            {
                if (commandUnitResult.IsFailure)
                    return ErrorResponse(httpContext,
                                         commandUnitResult.ParameterName,
                                         commandUnitResult.Error,
                                         commandUnitResult.XDebug);

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

            if (result is FileContentHttpResult)
                httpContext.Response.Headers.SetXStatus(XStatus.OK);

            return result;
        };
    }

    private static JsonHttpResult<ErrorResponse> ErrorResponse(
            HttpContext httpContext,
            string? parameterName,
            Error error,
            string? debugMessage
        )
    {
        var responseHeaders = httpContext.Response.Headers;
        responseHeaders.SetXStatus(XStatus.Error);

        if (debugMessage is not null)
            responseHeaders.SetXDebug(debugMessage);

        var errorCode = error.Code;

        var acceptLanguageHeaders = httpContext.Request.Headers.AcceptLanguage;
        if (acceptLanguageHeaders.Count == 0)
            return ErrorResponse(parameterName, error.Message, errorCode);

        var serviceProvider = httpContext.RequestServices;

        var langContext = serviceProvider.GetService<LanguageContext>();
        if (langContext is null)
            return ErrorResponse(parameterName, error.Message, errorCode);

        var acceptLanguageHeader = acceptLanguageHeaders.FirstOrDefault(h => h is not null);
        if (acceptLanguageHeader is null)
            return ErrorResponse(parameterName, error.Message, errorCode);

        var languages = AcceptLanguageHeaderHelper.Parse(acceptLanguageHeader);

        if (!langContext.Messages.TryGetValue(error.Key, out var message))
            return ErrorResponse(parameterName, error.Message, errorCode);

        var localizedMessage = message.Format(languages, error.Args) ?? error.Message;

        return ErrorResponse(parameterName, localizedMessage, errorCode);
    }

    private static JsonHttpResult<ErrorResponse> ErrorResponse(
            string? parameterName,
            string errorMessage,
            string? errorCode
        )
    {
        if (parameterName is null)
        {
            return TypedResults.Json(new ErrorResponse
            {
                Code    = errorCode,
                Message = errorMessage,
            });
        }

        var errorResponse = new ErrorResponse
        {
            Message = null,
            ParameterMessages = new Dictionary<string, string>
            {
                [parameterName] = errorMessage
            }
        };

        if (errorCode is not null)
        {
            errorResponse.ParameterCodes = new Dictionary<string, string>
            {
                [parameterName] = errorCode
            };
        }

        return TypedResults.Json(errorResponse);
    }
}