namespace GlavLib.Abstractions.Validation;

public struct Error
{
    public string Key { get; }

    public string Message { get; }

    public IDictionary<string, string>? Args { get; }

    public Error(string key, 
                 string message, 
                 IDictionary<string, string>? args = default)
    {
        Key = key;
        Message = message;
        Args = args;
    }

    public override string ToString()
    {
        return Message;
    }
}