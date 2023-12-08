using GlavLib.Abstractions.DI;

namespace GlavLib.Sandbox.API.Commands;

public interface ITestService
{
    string Foo();
}

[SingleInstance<ITestService>]
public class TestService : ITestService
{
    public string Foo()
    {
        return "Foo";
    }
}