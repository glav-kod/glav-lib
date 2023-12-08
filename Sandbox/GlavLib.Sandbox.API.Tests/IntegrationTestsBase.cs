using GlavGlavLib.Sandbox.API.Tests.TestDoubles;
using GlavLib.Sandbox.API.Commands;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Xunit.Abstractions;

namespace GlavGlavLib.Sandbox.API.Tests;

public abstract class IntegrationTestsBase : IAsyncLifetime
{
    protected TestWebApplicationFactory<Program> WebAppFactory { get; }

    public readonly TestServiceFake TestServiceFake = new();

    private readonly Stack<IDisposable> _disposables = new();

    protected IntegrationTestsBase(ITestOutputHelper testOutputHelper)
    {
        var logger = new LoggerConfiguration()
                     .WriteTo.TestOutput(testOutputHelper)
                     .CreateLogger();

        Using(logger);

        var webAppFactory = new TestWebApplicationFactory<Program>();

        webAppFactory.ConfigureServices(services =>
        {
            services.AddSingleton<ITestService>(TestServiceFake);
            
            services.AddLogging(config => //
            {
                config.AddSerilog(logger);
            });
        });

        WebAppFactory = webAppFactory;
    }

    protected void Using(IDisposable disposable)
    {
        _disposables.Push(disposable);
    }

    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        while (_disposables.TryPop(out var disposable))
            disposable.Dispose();

        return Task.CompletedTask;
    }
}