using GlavLib.Sandbox.Console;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();

services.Add_GlavLib_Sandbox_Console();

var serviceProvider = services.BuildServiceProvider();

var factory = serviceProvider.GetRequiredService<TestFactory>();

var c1 = factory.Create("c1");
var c2 = factory.Create("c2");

Console.WriteLine(c1);
Console.WriteLine(c2);

// var wrongSum = SystemErrors.WrongSum(sum: 123);
// Console.WriteLine(wrongSum.Key);
// Console.WriteLine(wrongSum.Message);
//
//
// CommandExecutor commandExecutor = null!;
//
// var result = await TestCommandHandler.HandleAsync(commandExecutor, new TestCommand(), default);
//
// [EnumObjectItem("KeyOne", "K1", "Ключ 1")]
// public partial class MyEnum : EnumObject
// {
// }

