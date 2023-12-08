using GlavLib.Abstractions.DI;
using Microsoft.Extensions.DependencyInjection;

namespace GlavLib.Sandbox.Console;

public interface ITest;

public class Class1 : ITest;

public class Class2 : ITest;

[SingleInstance, AddServicesFrom(nameof(RegisterServices))]
public class TestFactory(IServiceProvider serviceProvider)
{
    public ITest Create(string key)
    {
        return serviceProvider.GetRequiredKeyedService<ITest>(key);
    }

    public static void RegisterServices(IServiceCollection services)
    {
        services.AddKeyedSingleton<ITest, Class1>("c1");
        services.AddKeyedSingleton<ITest, Class2>("c2");
    }
}