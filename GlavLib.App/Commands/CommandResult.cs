using System.Diagnostics.CodeAnalysis;
using GlavLib.Abstractions.Validation;
using GlavLib.Errors;
using Microsoft.AspNetCore.Http;

namespace GlavLib.App.Commands;

public interface ICommandResult
{
    public bool IsFailure { get; }

    public IResult Value { get; }

    public Error Error { get; }

    public IDictionary<string, Error>? ParameterErrors { get; }

    public string? XDebug { get; }
}

public class CommandResult<TResult> : ICommandResult
    where TResult : IResult
{
    [MemberNotNullWhen(true, nameof(_error))]
    public bool IsFailure { get; }

    private readonly IResult? _value;
    public IResult Value => _value ?? throw new InvalidOperationException("Cannot get value of error result");

    private readonly Error? _error;
    public Error Error => _error ?? throw new InvalidOperationException("Cannot get error of success result");

    public IDictionary<string, Error>? ParameterErrors { get; }

    public string? XDebug { get; }

    private CommandResult(
            Error error,
            IDictionary<string, Error> parameterErrors,
            string? xDebug
        )
    {
        IsFailure       = true;
        _error          = error;
        ParameterErrors = parameterErrors;
        XDebug          = xDebug;
    }

    private CommandResult(Error error, string? xDebug)
    {
        IsFailure = true;
        _error    = error;
        XDebug    = xDebug;
    }

    private CommandResult(IResult value)
    {
        IsFailure = false;
        _value    = value;
    }

    public static implicit operator CommandResult<TResult>(TResult result)
    {
        return new CommandResult<TResult>(result);
    }

    public static implicit operator CommandResult<TResult>(Error error)
    {
        return new CommandResult<TResult>(error: error,
                                          xDebug: null);
    }

    public static implicit operator CommandResult<TResult>((string name, Error error) param)
    {
        var parameterErrors = new Dictionary<string, Error>
        {
            { param.name, param.error }
        };

        return new CommandResult<TResult>(error: BasicErrors.CheckFields,
                                          parameterErrors: parameterErrors,
                                          xDebug: null);
    }

    public static implicit operator CommandResult<TResult>(Dictionary<string, Error> parameterErrors)
    {
        return new CommandResult<TResult>(error: BasicErrors.CheckFields,
                                          parameterErrors: parameterErrors,
                                          xDebug: null);
    }

    public static implicit operator CommandResult<TResult>((Error error, string xDebug) result)
    {
        return new CommandResult<TResult>(error: result.error,
                                          xDebug: result.xDebug);
    }
}