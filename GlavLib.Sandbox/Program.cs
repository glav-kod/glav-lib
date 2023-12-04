using GlavLib.Basics.DataTypes;
using GlavLib.Sandbox;

Console.WriteLine(MyEnum.KeyOne);

var wrongSum = SystemErrors.WrongSum(sum: 123);
Console.WriteLine(wrongSum.Key);
Console.WriteLine(wrongSum.Message);


[EnumObjectItem("KeyOne", "K1", "Ключ 1")]
public partial class MyEnum : EnumObject
{
}
