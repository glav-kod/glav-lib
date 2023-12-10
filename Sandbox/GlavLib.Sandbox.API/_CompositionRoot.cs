﻿using GlavLib.Abstractions.DI;
using GlavLib.App;

namespace GlavLib.Sandbox.API;

[AddServicesFrom(nameof(RegisterServices))]
public static class CompositionRoot
{
    public static void RegisterServices(IServiceCollection services)
    {
        services.Add_GlavLib_App();
    }
}