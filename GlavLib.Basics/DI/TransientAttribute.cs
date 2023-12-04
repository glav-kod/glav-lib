using JetBrains.Annotations;

namespace GlavLib.Basics.DI;

[PublicAPI]
public sealed class TransientAttribute : Attribute
{
}

[PublicAPI]
public sealed class TransientAttribute<T> : Attribute
{
}