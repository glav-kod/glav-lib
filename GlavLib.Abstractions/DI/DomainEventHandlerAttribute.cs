namespace GlavLib.Abstractions.DI;

[AttributeUsage(AttributeTargets.Class)]
public sealed class DomainEventHandlerAttribute<T> : Attribute;