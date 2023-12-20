using JetBrains.Annotations;

namespace GlavLib.Abstractions.DI;

[AttributeUsage(AttributeTargets.Class), UsedImplicitly]
public sealed class SingleInstanceAttribute(object? key = null) : Attribute
{
    public object? Key { get; } = key;
}

[AttributeUsage(AttributeTargets.Class), UsedImplicitly]
public sealed class SingleInstanceAttribute<T>(object? key = null) : Attribute
{
    public object? Key { get; } = key;
}