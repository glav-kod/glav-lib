using GlavLib.App.Commands;

namespace GlavLib.App.Validation;

public sealed class ErrorResponse
{
    public required string Message { get; init; }

    public string? Code { get; init; }

    public Dictionary<string, string>? ParameterCodes { get; set; }

    public Dictionary<string, string>? ParameterMessages { get; set; }

    internal static ErrorResponse Create(
            LocalizedError error,
            Dictionary<string, LocalizedError>? parameterErrors
        )
    {
        var errorResponse = new ErrorResponse
        {
            Code    = error.Code,
            Message = error.Message
        };

        if (parameterErrors is null)
            return errorResponse;

        var parameterMessages = new Dictionary<string, string>();
        var parameterCodes    = new Dictionary<string, string>();

        foreach (var (parameterName, parameterError) in parameterErrors)
        {
            parameterMessages.Add(parameterName, parameterError.Message);

            if (parameterError.Code is not null)
                parameterCodes.Add(parameterName, parameterError.Code);
        }

        errorResponse.ParameterMessages = parameterMessages;
        errorResponse.ParameterCodes    = parameterCodes;

        return errorResponse;
    }
}