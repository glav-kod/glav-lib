namespace GlavLib.Abstractions.Results;

public readonly struct UnitResult<TError>
{
    public bool IsFailure { get; }

    public bool IsSuccess => !IsFailure;

    private readonly TError? _error;
    public TError Error => _error ?? throw new InvalidOperationException("Cannot access error in success result");

    internal UnitResult(bool isFailure, TError? error)
    {
        IsFailure = isFailure;
        _error    = error;
    }

    public static implicit operator UnitResult<TError>(TError error)
    {
        return new UnitResult<TError>(isFailure: true, error: error);
    }
}