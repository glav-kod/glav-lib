using GlavLib.Abstractions.Validation;
using Microsoft.AspNetCore.Http;

namespace GlavLib.App.Commands;

public interface ICommandResult
{
    public bool IsFailure { get; }

    public IResult Value { get; }

    public Error Error { get; }

    public string? ParameterName { get; }

    public string? XDebug { get; }
}

public class CommandResult<TResult> : ICommandResult
    where TResult : IResult
{
    public bool IsFailure { get; }

    private readonly IResult? _value;
    public IResult Value => _value ?? throw new InvalidOperationException("Cannot get value of error result");

    private readonly Error? _error;
    public Error Error => _error ?? throw new InvalidOperationException("Cannot get error of success result");

    public string? ParameterName { get; }

    public string? XDebug { get; }

    private CommandResult(string? parameterName, Error error, string? xDebug)
    {
        IsFailure     = true;
        ParameterName = parameterName;
        _error        = error;
        XDebug        = xDebug;
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
        return new CommandResult<TResult>(parameterName: null,
                                          error: error,
                                          xDebug: null);
    }

    public static implicit operator CommandResult<TResult>((string param, Error error) result)
    {
        return new CommandResult<TResult>(parameterName: result.param,
                                          error: result.error,
                                          xDebug: null);
    }

    public static implicit operator CommandResult<TResult>((Error error, string xDebug) result)
    {
        return new CommandResult<TResult>(parameterName: null,
                                          error: result.error,
                                          xDebug: result.xDebug);
    }
}