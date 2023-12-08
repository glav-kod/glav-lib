namespace GlavLib.Abstractions.DI;

[AttributeUsage(AttributeTargets.Class)]
public sealed class SingleInstanceAttribute : Attribute;

[AttributeUsage(AttributeTargets.Class)]
public sealed class SingleInstanceAttribute<T> : Attribute;