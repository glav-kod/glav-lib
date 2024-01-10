using FluentValidation;
using FluentValidation.Results;
using GlavLib.Abstractions.Results;
using GlavLib.Abstractions.Validation;

namespace GlavLib.Basics.Extensions;

public static class ErrorValidationExtensions
{
    public static IRuleBuilderOptions<T, TProperty> WithError<T, TProperty>(this IRuleBuilderOptions<T, TProperty> rule, Error error)
    {
        rule.WithMessage(error.Message)
            .WithState(_ => error);

        if (error.Code is not null)
            rule.WithErrorCode(error.Code);

        return rule;
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