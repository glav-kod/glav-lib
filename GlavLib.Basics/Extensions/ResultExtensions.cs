using System;
using CSharpFunctionalExtensions;

namespace GlavLib.Basics.Extensions;

public static class ResultExtensions
{
    public static void ThrowIfFailure<TResult, TError>(this Result<TResult, TError> result, Func<TError, string> messageFunc)
    {
        if (!result.IsFailure) 
            return;
        
        var message = messageFunc(result.Error);

        throw new InvalidOperationException(message)
        {
            Data =
            {
                ["Error"] = result.Error
            }
        };
    }

    public static void ThrowIfFailure<TError>(this UnitResult<TError> result, Func<TError, string> messageFunc)
    {
        if (!result.IsFailure) 
            return;
        var message = messageFunc(result.Error);
            
        throw new InvalidOperationException(message)
        {
            Data =
            {
                ["Error"] = result.Error
            }
        };
    }
}