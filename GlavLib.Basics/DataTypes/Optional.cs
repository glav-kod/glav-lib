namespace GlavLib.Basics.DataTypes;

internal interface IOptional
{
    public bool IsUndefined { get; }
}

public struct Optional<T> : IOptional
{
    public static readonly Optional<T> Undefined = default;

    private bool _hasValue;

    public T Value { get; private init; }

    public bool IsUndefined => !_hasValue;

    public T GetValue(T undefinedValue)
    {
        return IsUndefined 
            ? undefinedValue 
            : Value;
    }

    public override string ToString()
    {
        if (IsUndefined)
            return "<undefined>";

        if (Value is null)
            return "<null>";

        return Value.ToString()!;
    }

    public static implicit operator Optional<T>(T value)
    {
        return new Optional<T>
        {
            _hasValue = true,
            Value     = value
        };
    }
}