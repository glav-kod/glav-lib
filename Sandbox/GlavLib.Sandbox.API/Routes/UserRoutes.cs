using GlavLib.App.Commands;
using GlavLib.App.Db;
using GlavLib.Sandbox.API.Commands;
using GlavLib.Sandbox.API.Db;

namespace GlavLib.Sandbox.API.Routes;

public static class UserRoutes
{
    public static WebApplication MapUserRoutes(this WebApplication app, RouteGroupBuilder baseGroup)
    {
        var usersGroup = baseGroup.MapGroup("/users");

        usersGroup.MapGet("/get", GetUser.ExecuteAsync)
                  .AddDbSession(ConnectionStrings.Replica)
                  .UseCommands();
        
        usersGroup.MapPost("/create", CreateUser.ExecuteAsync)
                  .AddDbSession(ConnectionStrings.Master)
                  .UseCommands();

        return app;
    }
}