namespace GlavLib.Abstractions.DI;

[AttributeUsage(AttributeTargets.Class)]
public sealed class TransientAttribute : Attribute;

[AttributeUsage(AttributeTargets.Class)]
public sealed class TransientAttribute<T> : Attribute;