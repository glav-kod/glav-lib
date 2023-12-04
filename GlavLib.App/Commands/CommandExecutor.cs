using System.Diagnostics;
using Autofac;
using CSharpFunctionalExtensions;
using GlavLib.App.Db;
using GlavLib.App.DomainEvents;
using GlavLib.Basics.DI;
using GlavLib.Basics.DomainEvents;
using Microsoft.Extensions.Logging;

namespace GlavLib.App.Commands;

[Transient]
public sealed class CommandExecutor
{
    private static readonly DateTime TwentyOneCentury = new(2000, 1, 1);

    private readonly ILogger<CommandExecutor> _logger;
    private readonly ILifetimeScope           _serviceFactory;
    private readonly DomainEventsHandler      _domainEventsHandler;

    public CommandExecutor(ILogger<CommandExecutor> logger,
                           ILifetimeScope           serviceFactory,
                           DomainEventsHandler      domainEventsHandler)
    {
        _logger = logger;
        _serviceFactory = serviceFactory;
        _domainEventsHandler = domainEventsHandler;
    }

    public async Task<Result<TResult, CommandHandlerError>> ExecuteAsync<TCommand, TResult>(TCommand command, CancellationToken cancellationToken)
    {
        var commandNumber = (DateTime.UtcNow - TwentyOneCentury).Ticks;

        var commandId = $"{typeof(TCommand)}-{commandNumber}";

        using (_logger.BeginScope("CommandId#{CommandId}", commandId))
        {
            _logger.LogInformation("Обработка команды {Command}", typeof(TCommand).Name);

            var stopwatch = Stopwatch.StartNew();

            using var domainEventsSession = DomainEventsSession.Bind();

            var handler = _serviceFactory.Resolve<CommandHandler<TCommand, TResult>>();

            var result = await handler.HandleAsync(command, cancellationToken);

            if (result.IsFailure)
            {
                _logger.LogInformation("Обработка команды {Command} завершилась с ошибкой: {@Error}",
                                       typeof(TCommand).Name, result.Error);
            }
            else
            {
                await using var dbTransaction = new DbTransaction();

                await _domainEventsHandler.HandleAsync(domainEventsSession, cancellationToken);

                await dbTransaction.CommitAsync();

                _logger.LogInformation("Обработка команды {Command}#{@CommandArgs} завершилась успешно",
                                       typeof(TCommand).Name, command);
            }

            stopwatch.Stop();

            _logger.LogInformation("Обработка команды {Command} заняла {Time}", typeof(TCommand).Name,
                                   stopwatch.Elapsed);

            return result;
        }
    }

    public async Task<UnitResult<CommandHandlerError>> ExecuteAsync<TCommand>(TCommand command, CancellationToken cancellationToken)
    {
        var commandNumber = (DateTime.UtcNow - TwentyOneCentury).Ticks;

        var commandId = $"{typeof(TCommand)}-{commandNumber}";

        using (_logger.BeginScope("CommandId#{CommandId}", commandId))
        {
            _logger.LogInformation("Обработка команды {Command}", typeof(TCommand).Name);

            var stopwatch = Stopwatch.StartNew();

            using var domainEventsSession = DomainEventsSession.Bind();

            var handler = _serviceFactory.Resolve<UnitCommandHandler<TCommand>>();

            var result = await handler.HandleAsync(command, cancellationToken);

            if (result.IsFailure)
            {
                _logger.LogInformation("Обработка команды {Command} завершилась с ошибкой: {@Error}",
                                       typeof(TCommand).Name, result.Error);
            }
            else
            {
                await using var dbTransaction = new DbTransaction();

                await _domainEventsHandler.HandleAsync(domainEventsSession, cancellationToken);

                await dbTransaction.CommitAsync();

                _logger.LogInformation("Обработка команды {Command}#{@CommandArgs} завершилась успешно",
                                       typeof(TCommand).Name, command);
            }

            stopwatch.Stop();

            _logger.LogInformation("Обработка команды {Command} заняла {Time}", typeof(TCommand).Name,
                                   stopwatch.Elapsed);

            return result;
        }
    }
}