using JetBrains.Annotations;

namespace GlavLib.Abstractions.DI;

[AttributeUsage(AttributeTargets.Class), UsedImplicitly]
public sealed class TransientAttribute(object? key = null) : Attribute
{
    public object? Key { get; } = key;
}

[AttributeUsage(AttributeTargets.Class), UsedImplicitly]
public sealed class TransientAttribute<T>(object? key = null) : Attribute
{
    public object? Key { get; } = key;
}