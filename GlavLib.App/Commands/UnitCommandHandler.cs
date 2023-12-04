using CSharpFunctionalExtensions;

namespace GlavLib.App.Commands;

public abstract class UnitCommandHandler<TCommand>
{
    public abstract Task<UnitResult<CommandHandlerError>> HandleAsync(TCommand          command,
                                                                      CancellationToken cancellationToken);

    public static Task<UnitResult<CommandHandlerError>> HandleAsync(CommandExecutor   commandExecutor,
                                                                    TCommand          command,
                                                                    CancellationToken cancellationToken)
    {
        return commandExecutor.ExecuteAsync(command, cancellationToken);
    }
}