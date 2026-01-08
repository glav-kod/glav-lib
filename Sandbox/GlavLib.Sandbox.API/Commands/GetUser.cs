using FluentValidation;
using GlavLib.App.Commands;
using GlavLib.App.Http;
using GlavLib.Basics.Extensions;
using GlavLib.Db;
using GlavLib.Errors;
using GlavLib.Sandbox.API.Model;
using Microsoft.AspNetCore.Http.HttpResults;

namespace GlavLib.Sandbox.API.Commands;

public sealed class GetUserRequest
{
    public long UserId { get; init; }

    public sealed class Validator : AbstractValidator<GetUserRequest>
    {
        public Validator()
        {
            RuleFor(x => x.UserId).NotEmpty().WithError(BasicErrors.FillValue);
        }
    }
}

public sealed class UserDTO
{
    public required long Id { get; init; }

    public required string Name { get; init; }
}

public class GetUser
{
    public static async Task<CommandResult<Ok<UserDTO>>> ExecuteAsync(
            FromJsonQuery<GetUserRequest> request,
            CancellationToken cancellationToken
        )
    {
        var dbSession = StatefulDbSession.Current;
        var nhSession = dbSession.NhSession;

        var user = await nhSession.GetAsync<User>(request.Value.UserId, cancellationToken);

        if (user is null)
            return (ApiErrors.UserIsNotFound(request.Value.UserId), xDebug: $"User#{request.Value.UserId} is not found");

        var result = new UserDTO
        {
            Id   = user.Id,
            Name = user.Name
        };

        return Ok(result);
    }
}
