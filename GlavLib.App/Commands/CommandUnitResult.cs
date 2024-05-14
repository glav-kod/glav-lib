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

    public Error? ParameterError { get; }

    public string? ParameterName { get; }

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

    private CommandUnitResult(string parameterName, Error parameterError, string? xDebug)
    {
        IsFailure      = true;
        _error         = BasicErrors.CheckFields;
        ParameterName  = parameterName;
        ParameterError = parameterError;
        XDebug         = xDebug;
    }

    public static implicit operator CommandUnitResult(Error error)
    {
        return new CommandUnitResult(error, xDebug: null);
    }

    public static implicit operator CommandUnitResult((string paramName, Error paramError) result)
    {
        return new CommandUnitResult(parameterName: result.paramName,
                                     parameterError: result.paramError,
                                     xDebug: null);
    }

    public static implicit operator CommandUnitResult((Error error, string xDebug) result)
    {
        return new CommandUnitResult(error: result.error,
                                     xDebug: result.xDebug);
    }
}