using System;

namespace GlavLib.Basics.Errors;

public sealed class ErrorMessageAttribute : Attribute
{
    public string Name { get; }

    public string Message { get; }

    public ErrorMessageAttribute(string name, string message)
    {
        Name = name;
        Message = message;
    }
}