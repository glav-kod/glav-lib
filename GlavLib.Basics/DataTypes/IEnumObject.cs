namespace GlavLib.Basics.DataTypes;

public interface IEnumObject<out T> where T : IEnumObject<T>
{
    public string Key { get; }
    
    public static abstract T Create(string key);
}

