namespace GlavLib.Basics.DataTypes;

internal interface IOptional
{
    public bool HasValue { get; }
}

public readonly struct Optional<T> : IOptional
{
    public static readonly Optional<T> Undefined = new()
    {
        Value    = default,
        HasValue = false
    };

    public static readonly Optional<T> Null = new()
    {
        Value    = default,
        HasValue = true
    };

    public T? Value { get; private init; }

    public bool HasValue { get; private init; }

    public static implicit operator Optional<T>(T? value)
    {
        if (value is null)
            return Null;

        return new Optional<T>
        {
            HasValue = true,
            Value    = value
        };
    }

    public override string ToString()
    {
        if (!HasValue)
            return "<undefined>";

        if (Value is null)
            return "<null>";

        return Value.ToString()!;
    }
}