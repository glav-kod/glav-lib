using FluentValidation;
using GlavLib.Abstractions.DI;
using GlavLib.App.Commands;
using GlavLib.Basics.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;

namespace GlavLib.Sandbox.API.Commands;

public class CreatePaymentArgs
{
    public required string? Value { get; init; }

    public sealed class Validator : AbstractValidator<CreatePaymentArgs>
    {
        public Validator()
        {
            RuleFor(x => x.Value).NotNull().WithError(ApiErrors.FillValue);
        }
    }
}

public class CreatePaymentResult
{
    public required string? Value { get; set; }
}

[SingleInstance]
public sealed class PaymentCommands(TestService testService)
{
    public async Task<CommandResult<Ok<CreatePaymentResult>>> CreatePaymentAsync(CreatePaymentArgs args, CancellationToken ct)
    {
        await Task.Yield();

        if (args.Value == "error")
        {
            return ("asdasdas", ApiErrors.InvalidOperation);
        }
        
        return Ok(new CreatePaymentResult
        {
            Value = testService.Foo() + args.Value
        });
    }
}