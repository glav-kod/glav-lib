using GlavLib.Basics.Errors;

namespace GlavLib.App.Commands;

public struct CommandHandlerError
{
    public required ErrorMessage Error { get; init; }

    public string? ParameterName { get; init; }

    public static implicit operator CommandHandlerError(Tuple<ErrorMessage> errorMessage)
    {
        return new CommandHandlerError
        {
            Error = errorMessage.Item1
        };
    }
}