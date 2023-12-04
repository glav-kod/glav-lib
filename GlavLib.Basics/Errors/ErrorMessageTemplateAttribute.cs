using System;

namespace GlavLib.Basics.Errors;

public sealed class ErrorMessageTemplateAttribute : Attribute
{
    public string Name { get; }

    public string Message { get; }

    public ErrorMessageTemplateAttribute(string name, string message)
    {
        Name = name;
        Message = message;
    }
}