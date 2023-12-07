using GlavLib.Sandbox.API.Commands;

namespace GlavLib.Sandbox.API.Routes;

public static class UserRoutes
{
    public static WebApplication MapUserRoutes(this WebApplication app, RouteGroupBuilder baseGroup)
    {
        var usersGroup = baseGroup.MapGroup("/users");

        usersGroup.MapPost("/create", UserCommands.CreateAsync);
        usersGroup.MapPost("/delete", UserCommands.DeleteAsync);

        return app;
    }
}