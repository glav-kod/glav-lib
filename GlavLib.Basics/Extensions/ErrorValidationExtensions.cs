using FluentValidation;
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
}