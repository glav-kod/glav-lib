using FluentValidation;
using FluentValidation.Results;
using GlavLib.Abstractions.Results;
using GlavLib.Abstractions.Validation;

namespace GlavLib.Basics.Extensions;

public static class ErrorValidationExtensions
{
    public static IRuleBuilderOptions<T, TProperty> WithError<T, TProperty>(this IRuleBuilderOptions<T, TProperty> rule, Error error)
    {
        return rule.WithErrorCode(error.Code)
                   .WithMessage(error.Message)
                   .WithState(_ => error);
    }

    public static IRuleBuilderOptions<T, TProperty> MustBeValueObject<T, TValueObject, TProperty>(
            this IRuleBuilder<T, TProperty> ruleBuilder,
            Func<TProperty, Result<TValueObject, Error>> factoryMethod
        )
    {
        return (IRuleBuilderOptions<T, TProperty>)ruleBuilder.Custom((value, context) =>
        {
            var result = factoryMethod(value!);

            if (!result.IsFailure)
                return;

            var error = result.Error;

            context.AddFailure(new ValidationFailure
            {
                ErrorCode    = error.Code,
                ErrorMessage = error.Message,
                CustomState  = error
            });
        });
    }
}