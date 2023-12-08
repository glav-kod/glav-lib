using FluentValidation;
using GlavLib.App.Commands;
using GlavLib.App.Http;
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

public class GetUserRequest
{
    public string Field1 { get; init; } = null!;

    public string Field2 { get; init; } = null!;

    public sealed class Validator : AbstractValidator<GetUserRequest>
    {
        public Validator()
        {
            RuleFor(x => x.Field1).NotNull().WithError(ApiErrors.FillValue);

            RuleFor(x => x.Field2).NotNull().WithError(ApiErrors.FillValue);
        }
    }
}

public class UserCommands
{
    public static async Task<CommandResult<Ok>> GetAsync(FromJsonQuery<GetUserRequest> request,
                                                         CancellationToken             cancellationToken)
    {
        await Task.Yield();

        return Ok();

        // return ApiErrors.FillValue;
        // return (nameof(CreateUserArgs.Value), ApiErrors.FillValue);
    }

    public static async Task<CommandResult<Ok<CreateUserResult>>> CreateAsync(CreateUserArgs        args,
                                                                              ILogger<UserCommands> logger,
                                                                              ITestService          testService,
                                                                              CancellationToken     cancellationToken)
    {
        await Task.Yield();

        var result = testService.Foo();

        logger.LogInformation("User created");

        return Ok(new CreateUserResult
        {
            Value = $"{args.Value} {result}"
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