using GlavLib.Abstractions.DI;
using GlavLib.App;
using GlavLib.Db;

namespace GlavLib.Sandbox.API;

[AddServicesFrom(nameof(RegisterServices))]
public static class ReferenceServicesRegistration
{
    public static void RegisterServices(IServiceCollection services)
    {
        services.Add_GlavLib_App();
        services.Add_GlavLib_Db();
    }
}