namespace GlavLib.Abstractions.Validation;

public readonly struct Error
{
    public Error(
            string key,
            string? code,
            string message,
            IDictionary<string, string>? args = null
        )
    {
        Key     = key;
        Code    = code;
        Message = message;
        Args    = args;
    }

    public Error(string message)
    {
        Message = message;
    }

    public string Message { get; }

    public string? Key { get; }

    public string? Code { get; }

    public IDictionary<string, string>? Args { get; }

    public static implicit operator Error(string message)
    {
        return new Error(message);
    }

    public override string ToString()
    {
        return Message;
    }
}