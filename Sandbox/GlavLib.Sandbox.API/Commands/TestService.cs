using GlavLib.Abstractions.DI;

namespace GlavLib.Sandbox.API.Commands;

[SingleInstance]
public class TestService
{
    public string Foo()
    {
        return "Foo";
    }
}