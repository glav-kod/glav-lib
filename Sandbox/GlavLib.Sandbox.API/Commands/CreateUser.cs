﻿using FluentValidation;
using GlavLib.App.Commands;
using GlavLib.App.Db;
using GlavLib.Basics;
using GlavLib.Basics.Extensions;
using GlavLib.Sandbox.API.Model;
using Microsoft.AspNetCore.Http.HttpResults;

namespace GlavLib.Sandbox.API.Commands;

public class CreateUserArgs
{
    public required string Name { get; init; }

    public sealed class Validator : AbstractValidator<CreateUserArgs>
    {
        public Validator()
        {
            RuleFor(x => x.Name).NotNull().WithError(BasicErrors.FillValue);
        }
    }
}

public class CreateUserResult
{
    public required long UserId { get; set; }
}

public class CreateUser
{
    public static async Task<CommandResult<Ok<CreateUserResult>>> ExecuteAsync(CreateUserArgs      args,
                                                                               ILogger<CreateUser> logger,
                                                                               CancellationToken   cancellationToken)
    {
        var nhSession = DbSession.CurrentNhSession;

        await using var dbTransaction = new DbTransaction();

        var user = User.Create(args.Name);
        await nhSession.SaveAsync(user, cancellationToken);
        await nhSession.FlushAsync(cancellationToken);

        await dbTransaction.CommitAsync();

        logger.LogInformation("User#{UserId} created", user.Id);

        return Ok(new CreateUserResult
        {
            UserId = user.Id
        });
    }
}