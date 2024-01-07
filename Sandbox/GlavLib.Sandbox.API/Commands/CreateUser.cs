﻿using System.Xml.Serialization;
using FluentValidation;
using GlavLib.App.Commands;
using GlavLib.App.Http;
using GlavLib.Basics.Extensions;
using GlavLib.Db;
using GlavLib.Errors;
using GlavLib.Sandbox.API.Model;

namespace GlavLib.Sandbox.API.Commands;

[XmlRoot("root")]
public class CreateUserArgs
{
    [XmlElement("name")]
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
    public static async Task<IResult> ExecuteAsync(
            FromXml<CreateUserArgs> args,
            ILogger<CreateUser> logger,
            CancellationToken cancellationToken
        )
    {
        var nhSession = DbSession.CurrentNhSession;

        using var dbTransaction = new DbTransaction();

        var user = User.Create(args.Value.Name);
        await nhSession.SaveAsync(user, cancellationToken);
        await nhSession.FlushAsync(cancellationToken);

        dbTransaction.Commit();

        logger.LogInformation("User#{UserId} created", user.Id);

        return new CreateUserResult
        {
            UserId = user.Id
        }.AsXml();
    }
}