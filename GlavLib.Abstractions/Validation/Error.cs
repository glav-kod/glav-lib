namespace GlavLib.Abstractions.Validation;

public readonly struct Error(
        string message,
        string? key = null,
        string? code = null,
        IDictionary<string, string>? args = null
    )
{
    public string Message { get; } = message;

    public string? Key { get; } = key;

    public string? Code { get; } = code;

    public IDictionary<string, string>? Args { get; } = args;

    public static implicit operator Error(string message)
    {
        return new Error(message);
    }

    public override string ToString()
    {
        return Message;
    }
}