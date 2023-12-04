using System;

namespace GlavLib.Basics.DataTypes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class EnumObjectItemAttribute : Attribute
{
    public string FieldName { get; set; }

    public string Value { get; set; }

    public string DisplayName { get; set; }

    public EnumObjectItemAttribute(string fieldName, string value, string displayName)
    {
        FieldName = fieldName;
        Value = value;
        DisplayName = displayName;
    }
}