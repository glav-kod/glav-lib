namespace GlavLib.Abstractions.DataTypes;

public abstract class EnumObject(
        string key,
        string displayName
    )
{
    public string Key { get; } = key;

    public string DisplayName { get; } = displayName;

    public override string ToString()
    {
        return DisplayName;
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
}