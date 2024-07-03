namespace GlavLib.Abstractions.DataTypes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class EnumObjectItemAttribute : Attribute
{
    public string FieldName { get; set; }

    public string Value { get; set; }

    public string DisplayName { get; set; }

    public string? RefStaticFieldName { get; set; }

    public EnumObjectItemAttribute(
            string fieldName,
            string value,
            string displayName,
            string? refStaticFieldName = null
        )
    {
        FieldName          = fieldName;
        Value              = value;
        DisplayName        = displayName;
        RefStaticFieldName = refStaticFieldName;
    }
}