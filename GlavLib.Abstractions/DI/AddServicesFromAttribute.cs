namespace GlavLib.Abstractions.DI;

[AttributeUsage(AttributeTargets.Class)]
public sealed class AddServicesFromAttribute(string methodName) : Attribute
{
    public string MethodName { get; } = methodName;
}