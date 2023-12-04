using CSharpFunctionalExtensions;
using GlavLib.App.Commands;
using GlavLib.App.DomainEvents;
using GlavLib.Basics.DomainEvents;

namespace GlavLib.Sandbox;

public sealed class TestEvent : DomainEvent  { }

public sealed class TestEventHandler : DomainEventHandler<TestEvent>
{
    protected override Task HandleAsync(TestEvent domainEvent, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}

public class TestCommand
{
}

public sealed class TestCommandHandler : CommandHandler<TestCommand, long>
{
    public override Task<Result<long, CommandHandlerError>> HandleAsync(TestCommand command, CancellationToken cancellationToken)
    {
        Result<long, CommandHandlerError> result = 1;
        return Task.FromResult(result);
    }
}