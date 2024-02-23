namespace GlavLib.Abstractions.DataTypes;

public abstract class EnumObject
{
    public string Key { get; }

    public string DisplayName { get; }

    protected EnumObject(string key, string displayName)
    {
        Key         = key;
        DisplayName = displayName;
    }

    public override string ToString()
    {
        return DisplayName;
    }

    private bool Equals(EnumObject other)
    {
        return Key == other.Key;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj))
            return false;

        if (ReferenceEquals(this, obj))
            return true;

        if (obj.GetType() != GetType())
            return false;

        var other = (EnumObject)obj;
        return Key == other.Key;
    }

    public override int GetHashCode()
    {
        return Key.GetHashCode();
    }

    public static bool Equals(EnumObject? a, EnumObject? b)
    {
        if (a is null && b is null)
            return true;

        if (a is null || b is null)
            return false;

        return a.Equals(b);
    }

    public static bool operator ==(EnumObject? left, EnumObject? right)
    {
        if (left is null && right is null)
            return true;

        if (left is null || right is null)
            return false;

        return left.Equals(right);
    }

    public static bool operator !=(EnumObject? left, EnumObject? right)
    {
        if (left is null && right is null)
            return false;

        if (left is null || right is null)
            return true;

        return !left.Equals(right);
    }
}