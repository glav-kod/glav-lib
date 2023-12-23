// ReSharper disable NonReadonlyMemberInGetHashCode

namespace GlavLib.Abstractions.ValueObjects;

[Serializable]
public abstract class ValueObject : IComparable, IComparable<ValueObject>
{
    private int? _cachedHashCode;

    protected abstract IEnumerable<IComparable> GetEqualityComponents();

    public override bool Equals(object? obj)
    {
        if (obj == null)
            return false;

        if (GetUnproxiedType(this) != GetUnproxiedType(obj))
            return false;

        var valueObject = (ValueObject)obj;

        return GetEqualityComponents().SequenceEqual(valueObject.GetEqualityComponents());
    }

    public override int GetHashCode()
    {
        if (!_cachedHashCode.HasValue)
        {
            _cachedHashCode = GetEqualityComponents()
                .Aggregate(1,
                           (current, obj) =>
                           {
                               unchecked
                               {
                                   return current * 23 + obj.GetHashCode();
                               }
                           });
        }

        return _cachedHashCode.Value;
    }

    public virtual int CompareTo(ValueObject? other)
    {
        if (other is null)
            return 1;

        if (ReferenceEquals(this, other))
            return 0;

        var thisType  = GetUnproxiedType(this);
        var otherType = GetUnproxiedType(other);
        if (thisType != otherType)
            return string.Compare($"{thisType}", $"{otherType}", StringComparison.Ordinal);

        return
            GetEqualityComponents().Zip(other.GetEqualityComponents(),
                                        (left, right) => left.CompareTo(right))
                                   .FirstOrDefault(cmp => cmp != 0);
    }

    public virtual int CompareTo(object? other)
    {
        return CompareTo(other as ValueObject);
    }

    public static bool operator ==(ValueObject? a, ValueObject? b)
    {
        if (a is null && b is null)
            return true;

        if (a is null || b is null)
            return false;

        return a.Equals(b);
    }

    public static bool operator !=(ValueObject a, ValueObject b)
    {
        return !(a == b);
    }

    internal static Type? GetUnproxiedType(object? obj)
    {
        if (obj is null)
            return null;

        const string efCoreProxyPrefix      = "Castle.Proxies.";
        const string nHibernateProxyPostfix = "Proxy";

        Type   type       = obj.GetType();
        string typeString = type.ToString();

        if (typeString.Contains(efCoreProxyPrefix) || typeString.EndsWith(nHibernateProxyPostfix))
            return type.BaseType!;

        return type;
    }
}