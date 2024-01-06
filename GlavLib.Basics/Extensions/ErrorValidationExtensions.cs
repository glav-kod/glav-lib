using FluentValidation;
using FluentValidation.Results;
using GlavLib.Abstractions.Results;
using GlavLib.Abstractions.Validation;

namespace GlavLib.Basics.Extensions;

public static class ErrorValidationExtensions
{
    public static IRuleBuilderOptions<T, TProperty> WithError<T, TProperty>(this IRuleBuilderOptions<T, TProperty> rule, Error error)
    {
        return rule.WithErrorCode(error.Key)
                   .WithState(_ => error.Args)
                   .WithMessage(error.Message);
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
                ErrorCode    = error.Key,
                CustomState  = error.Args,
                ErrorMessage = error.Message
            });
        });
    }
}