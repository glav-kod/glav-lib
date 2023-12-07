using GlavLib.Sandbox.API.Commands;

namespace GlavLib.Sandbox.API.Routes;

public static class PaymentRoutes
{
    public static WebApplication MapPaymentRoutes(this WebApplication app, RouteGroupBuilder baseGroup)
    {
        var commands = app.Services.GetRequiredService<PaymentCommands>();

        var paymentsGroup = baseGroup.MapGroup("/payments");

        paymentsGroup.MapPost("/create", commands.CreatePaymentAsync);

        return app;
    }
}