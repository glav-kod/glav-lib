using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace GlavGlavLib.Sandbox.API.Tests;

public class TestWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram>
    where TProgram : class
{
    private Action<IServiceCollection>? _configureServices;

    public TestWebApplicationFactory<TProgram> ConfigureServices(Action<IServiceCollection> configure)
    {
        _configureServices = configure;
        return this;
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.ConfigureServices(services => //
        {
            _configureServices?.Invoke(services);
        });

        return base.CreateHost(builder);
    }
}