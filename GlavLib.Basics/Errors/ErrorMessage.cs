namespace GlavLib.Basics.Errors;

public struct ErrorMessage
{
    public string Key { get; }

    public string Message { get; }

    public object? Args { get; }

    public ErrorMessage(string key, string message, object? args = default)
    {
        Key = key;
        Message = message;
        Args = args;
    }
}