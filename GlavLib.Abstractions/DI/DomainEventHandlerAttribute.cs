using JetBrains.Annotations;

namespace GlavLib.Abstractions.DI;

[AttributeUsage(AttributeTargets.Class), UsedImplicitly]
public sealed class DomainEventHandlerAttribute<T> : Attribute;