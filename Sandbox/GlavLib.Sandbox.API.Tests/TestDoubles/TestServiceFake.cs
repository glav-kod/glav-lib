using GlavLib.Sandbox.API.Commands;

namespace GlavGlavLib.Sandbox.API.Tests.TestDoubles;

public class TestServiceFake : ITestService
{
    private string _foo = string.Empty;
    
    public void SetFoo(string value)
    {
        _foo = value;
    }
    
    public string Foo()
    {
        return _foo;
    }
}