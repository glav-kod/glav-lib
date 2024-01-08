namespace GlavLib.Abstractions.Validation;

public struct Error(
        string key,
        string? code,
        string message,
        IDictionary<string, string>? args = default
    )
{
    public string Key { get; } = key;

    public string? Code { get; } = code;

    public string Message { get; } = message;

    public IDictionary<string, string>? Args { get; } = args;

    public override string ToString()
    {
        return Message;
    }
}