using System.Diagnostics.CodeAnalysis;
using GlavLib.Abstractions.Validation;
using GlavLib.Errors;
using JetBrains.Annotations;

namespace GlavLib.App.Commands;

public class CommandUnitResult
{
    [PublicAPI] public static readonly CommandUnitResult Success = new();

    [MemberNotNullWhen(true, nameof(_error))]
    public bool IsFailure { get; }

    private readonly Error? _error;
    public Error Error => _error ?? throw new InvalidOperationException("Cannot get error of success result");

    public IDictionary<string, Error>? ParameterErrors { get; }

    public string? XDebug { get; }

    private CommandUnitResult()
    {
        IsFailure = false;
    }

    private CommandUnitResult(Error error, string? xDebug)
    {
        IsFailure = true;
        _error    = error;
        XDebug    = xDebug;
    }

    private CommandUnitResult(IDictionary<string, Error> parameterErrors, string? xDebug)
    {
        IsFailure       = true;
        _error          = BasicErrors.CheckFields;
        ParameterErrors = parameterErrors;
        XDebug          = xDebug;
    }

    public static implicit operator CommandUnitResult(Error error)
    {
        return new CommandUnitResult(error, xDebug: null);
    }

    public static implicit operator CommandUnitResult((string paramName, Error paramError) result)
    {
        var parameterErrors = new Dictionary<string, Error>
        {
            { result.paramName, result.paramError }
        };

        return new CommandUnitResult(parameterErrors: parameterErrors, xDebug: null);
    }

    public static implicit operator CommandUnitResult(Dictionary<string, Error> parameterErrors)
    {
        return new CommandUnitResult(parameterErrors: parameterErrors, xDebug: null);
    }

    public static implicit operator CommandUnitResult((Error error, string xDebug) result)
    {
        return new CommandUnitResult(error: result.error,
                                     xDebug: result.xDebug);
    }
}