using Dapper;
using FluentValidation;
using GlavLib.App.Commands;
using GlavLib.App.Http;
using GlavLib.Basics.Extensions;
using GlavLib.Db;
using GlavLib.Errors;
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
    public static async Task<CommandResult<Ok<UserDTO>>> ExecuteAsync(FromJsonQuery<GetUserRequest> request,
                                                                      CancellationToken             cancellationToken)
    {
        var connection = DbSession.CurrentConnection;

        const string sql = """
                           select t.id,
                                  t.name
                             from public.users t
                            where t.id = :id
                           """;

        var parameters = new
        {
            id = request.Value.UserId
        };

        var user = await connection.QuerySingleOrDefaultAsync<UserDTO>(sql, parameters);

        if (user is null)
            return ("p1", ApiErrors.UserIsNotFound(request.Value.UserId));

        return Ok(user);
    }
}