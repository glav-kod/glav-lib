using JetBrains.Annotations;

namespace GlavLib.Abstractions.Results;

[PublicAPI]
public static class Result
{
    public static Result<TValue, TError> Failure<TValue, TError>(TError error) => new(isFailure: true,
                                                                                      value: default!,
                                                                                      error: error);

    public static Result<TValue, TError> Success<TValue, TError>(TValue value) => new(isFailure: false,
                                                                                      value: value,
                                                                                      error: default);

    public static UnitResult<TError> Failure<TError>(TError error) => new(isFailure: true,
                                                                          error: error);

    public static UnitResult<TError> Success<TError>() => new(isFailure: false,
                                                              error: default);
}

public readonly struct Result<TValue, TError>
{
    public bool IsFailure { get; }

    public bool IsSuccess => !IsFailure;

    private readonly TValue _value;

    public TValue Value => IsSuccess
        ? _value
        : throw new InvalidOperationException("Cannot access value in error result");

    private readonly TError? _error;
    public TError Error => _error ?? throw new InvalidOperationException("Cannot access error in success result");

    internal Result(bool isFailure, TValue value, TError? error)
    {
        IsFailure = isFailure;
        _value    = value;
        _error    = error;
    }

    public static implicit operator Result<TValue, TError>(TValue value)
    {
        return new Result<TValue, TError>(isFailure: false,
                                          value: value,
                                          error: default);
    }

    public static implicit operator Result<TValue, TError>(TError error)
    {
        return new Result<TValue, TError>(isFailure: true,
                                          value: default!,
                                          error: error);
    }
}