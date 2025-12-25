using GlavLib.App.Commands;
using GlavLib.Sandbox.API.Commands;

namespace GlavLib.Sandbox.API.Routes;

public static class ErrorRoutes
{
    public static WebApplication MapErrorRoutes(this WebApplication app, RouteGroupBuilder baseGroup)
    {
        var usersGroup = baseGroup.MapGroup("/errors");

        usersGroup.MapGet("/get-invalid-operation", ErrorCommands.GetInvalidOperation)
                  .UseCommands();

        usersGroup.MapGet("/get-client-formatted-error", ErrorCommands.GetClientFormattedError)
                  .UseCommands();

        return app;
    }
}