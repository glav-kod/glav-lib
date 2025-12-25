using GlavLib.App.Commands;
using Microsoft.AspNetCore.Mvc;

namespace GlavLib.Sandbox.API.Commands;

public sealed class ErrorCommands
{
    public static CommandUnitResult GetInvalidOperation()
    {
        return ApiErrors.InvalidOperation;
    }

    public static CommandUnitResult GetClientFormattedError([FromQuery] string message)
    {
        return ApiErrors.ClientFormattedError(message);
    }
}