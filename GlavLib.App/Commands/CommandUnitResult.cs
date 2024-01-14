using GlavLib.Abstractions.Validation;

namespace GlavLib.App.Commands;

public class CommandUnitResult
{
    public static readonly CommandUnitResult Success = new();

    public bool IsFailure { get; }

    private readonly Error? _error;
    public Error Error => _error ?? throw new InvalidOperationException("Cannot get error of success result");

    public string? ParameterName { get; }

    public string? XDebug { get; }

    private CommandUnitResult()
    {
        IsFailure = false;
    }

    private CommandUnitResult(string? parameterName, Error error, string? xDebug)
    {
        IsFailure     = true;
        ParameterName = parameterName;
        _error        = error;
        XDebug        = xDebug;
    }

    public static implicit operator CommandUnitResult(Error error)
    {
        return new CommandUnitResult(parameterName: null,
                                     error,
                                     xDebug: null);
    }

    public static implicit operator CommandUnitResult((string param, Error error) result)
    {
        return new CommandUnitResult(parameterName: result.param,
                                     error: result.error,
                                     xDebug: null);
    }

    public static implicit operator CommandUnitResult((Error error, string xDebug) result)
    {
        return new CommandUnitResult(parameterName: null,
                                     error: result.error,
                                     xDebug: result.xDebug);
    }
}