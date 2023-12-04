namespace GlavLib.Basics.DataTypes;

public enum MessageSeverity
{
    Success = 1,
    Warning = 2,
    Error = 3
}

public class Message
{
    public string Text { get; }

    public MessageSeverity Severity { get; }

    public Message(string text, MessageSeverity severity)
    {
        Text     = text;
        Severity = severity;
    }

    public static Message Success(string message)
    {
        return new Message(message, MessageSeverity.Success);
    }

    public static Message Warning(string message)
    {
        return new Message(message, MessageSeverity.Warning);
    }

    public static Message Error(string message)
    {
        return new Message(message, MessageSeverity.Error);
    }
}