using FluentValidation;
using GlavLib.App.Commands;
using GlavLib.Basics.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;

namespace GlavLib.Sandbox.API.Commands;

public class CreateUserArgs
{
    public required string? Value { get; init; }

    public sealed class Validator : AbstractValidator<CreateUserArgs>
    {
        public Validator()
        {
            RuleFor(x => x.Value).NotNull().WithError(ApiErrors.FillValue);
        }
    }
}

public class CreateUserResult
{
    public required string? Value { get; set; }
}

public static class UserCommands
{
    public static async Task<CommandResult<Ok<CreateUserResult>>> CreateAsync(CreateUserArgs    command,
                                                                               TestService       testService,
                                                                               CancellationToken cancellationToken)
    {
        await Task.Yield();

        var result = testService.Foo();

        return Ok(new CreateUserResult
        {
            Value = result
        });

        // return ApiErrors.FillValue;
        // return (nameof(CreateUserArgs.Value), ApiErrors.FillValue);
    }

    public static async Task<CommandUnitResult> DeleteAsync(CancellationToken cancellationToken)
    {
        await Task.Yield();

        // return EndpointUnitResult.Success;

        // return ApiErrors.FillValue;
        return ("someParameter", ApiErrors.FillValue);
    }
}