using GlavLib.Sandbox;

var wrongSum = SystemErrors.WrongSum(1.2222m);
Console.WriteLine(wrongSum.Message);

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
