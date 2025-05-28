namespace GlavLib.Basics.DataTypes;

public abstract class SingleValueObject<TValue>
{
    public TValue Value { get; }

    protected SingleValueObject(TValue value)
    {
        Value = value;
    }

    private bool Equals(SingleValueObject<TValue> other)
    {
        if (UnproxyType(this) != UnproxyType(other))
            return false;

        return Value!.Equals(other.Value);
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj))
            return false;

        if (ReferenceEquals(this, obj))
            return true;

        return Equals((SingleValueObject<TValue>)obj);
    }

    public override int GetHashCode()
    {
        return Value!.GetHashCode();
    }

    public override string ToString()
    {
        return Value!.ToString()!;
    }

    public static bool operator ==(SingleValueObject<TValue>? a, SingleValueObject<TValue>? b)
    {
        if (a is null && b is null)
            return true;

        if (a is null || b is null)
            return false;

        return a.Equals(b);
    }

    public static bool operator !=(SingleValueObject<TValue>? a, SingleValueObject<TValue>? b)
    {
        if (a is null && b is null)
            return false;

        if (a is null || b is null)
            return true;

        return !a.Equals(b);
    }

    private static Type UnproxyType(object obj)
    {
        var type = obj.GetType();
        var str  = type.ToString();

        return str.Contains("Castle.Proxies.") || str.EndsWith("Proxy")
            ? type.BaseType!
            : type;
    }
}