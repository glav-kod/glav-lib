using GlavLib.Abstractions.Validation;
using Microsoft.AspNetCore.Http;

namespace GlavLib.App.Commands;

public interface ICommandResult
{
    public bool IsFailure { get; }

    public IResult Value { get; }

    public Error Error { get; }

    public string? ParameterName { get; }
}

public class CommandResult<TResult> : ICommandResult
    where TResult : IResult
{
    public bool IsFailure { get; }

    private IResult? _value;
    public  IResult  Value => _value ?? throw new InvalidOperationException("Cannot get value of error result");

    private readonly Error? _error;
    public           Error  Error => _error ?? throw new InvalidOperationException("Cannot get error of success result");

    public string? ParameterName { get; }

    private CommandResult(string? parameterName, Error error)
    {
        IsFailure = true;
        ParameterName = parameterName;
        _error = error;
    }

    private CommandResult(IResult value)
    {
        IsFailure = false;
        _value = value;
    }

    public static implicit operator CommandResult<TResult>(TResult result)
    {
        return new CommandResult<TResult>(result);
    }

    public static implicit operator CommandResult<TResult>(Error error)
    {
        return new CommandResult<TResult>(parameterName: null,
                                          error: error);
    }

    public static implicit operator CommandResult<TResult>((string parameterName, Error error) result)
    {
        return new CommandResult<TResult>(parameterName: result.parameterName,
                                          error: result.error);
    }
}