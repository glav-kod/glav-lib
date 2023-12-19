using JetBrains.Annotations;

namespace GlavLib.Abstractions.DI;

[AttributeUsage(AttributeTargets.Class), UsedImplicitly]
public sealed class TransientAttribute : Attribute;

[AttributeUsage(AttributeTargets.Class), UsedImplicitly]
public sealed class TransientAttribute<T> : Attribute;