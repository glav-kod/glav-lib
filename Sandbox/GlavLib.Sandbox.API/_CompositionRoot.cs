using GlavLib.Abstractions.DI;
using GlavLib.App;

namespace GlavLib.Sandbox.API;

public class CompositionRoot : IHaveServiceRegistrations
{
    public static void RegisterServices(IServiceCollection services)
    {
        services.Add_GlavLib_App();
    }
}