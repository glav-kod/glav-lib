using JetBrains.Annotations;

namespace GlavLib.Abstractions.DI;

[AttributeUsage(AttributeTargets.Class), UsedImplicitly]
public sealed class SingleInstanceAttribute : Attribute;

[AttributeUsage(AttributeTargets.Class), UsedImplicitly]
public sealed class SingleInstanceAttribute<T> : Attribute;