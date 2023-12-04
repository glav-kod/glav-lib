using CSharpFunctionalExtensions;

namespace GlavLib.App.Commands;

public abstract class CommandHandler<TCommand, TResult>
{
    public abstract Task<Result<TResult, CommandHandlerError>> HandleAsync(TCommand          command,
                                                                   CancellationToken cancellationToken);

    public static Task<Result<TResult, CommandHandlerError>> HandleAsync(CommandExecutor   commandExecutor,
                                                                 TCommand          command,
                                                                 CancellationToken cancellationToken)
    {
        return commandExecutor.ExecuteAsync<TCommand, TResult>(command, cancellationToken);
    }
}