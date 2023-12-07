using GlavLib.Abstractions.Validation;

namespace GlavLib.App.Commands;

public class CommandUnitResult
{
    public static readonly CommandUnitResult Success = new();

    public bool IsFailure { get; }

    private readonly Error? _error;
    public           Error  Error => _error ?? throw new InvalidOperationException("Cannot get error of success result");

    public string? ParameterName { get; }

    private CommandUnitResult()
    {
        IsFailure = false;
    }

    private CommandUnitResult(string? parameterName, Error error)
    {
        IsFailure = true;
        ParameterName = parameterName;
        _error = error;
    }

    public static implicit operator CommandUnitResult(Error error)
    {
        return new CommandUnitResult(parameterName: null, error);
    }

    public static implicit operator CommandUnitResult((string parameterName, Error error) result)
    {
        return new CommandUnitResult(parameterName: result.parameterName,
                                     error: result.error);
    }
}