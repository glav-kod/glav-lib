using JetBrains.Annotations;

namespace GlavLib.Basics.DI;

[PublicAPI]
public sealed class SingleInstanceAttribute : Attribute
{
}

[PublicAPI]
public sealed class SingleInstanceAttribute<T> : Attribute
{
}