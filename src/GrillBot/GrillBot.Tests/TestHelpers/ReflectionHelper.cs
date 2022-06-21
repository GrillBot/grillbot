using System;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace GrillBot.Tests.TestHelpers;

public static class AutoPropertyExtensions
{
    private const string Prefix = "<";
    private const string Suffix = ">k__BackingField";

    private static string GetBackingFieldName(string propertyName) => $"{Prefix}{propertyName}{Suffix}";

    public static FieldInfo GetBackingField(this PropertyInfo propertyInfo)
    {
        if (propertyInfo == null)
            throw new ArgumentNullException(nameof(propertyInfo));
        if (!propertyInfo.CanRead || !propertyInfo.GetGetMethod(nonPublic: true)!.IsDefined(typeof(CompilerGeneratedAttribute), inherit: true))
            return null;
        var backingFieldName = GetBackingFieldName(propertyInfo.Name);
        var backingField = propertyInfo.DeclaringType?.GetField(backingFieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        if (backingField == null)
            return null;
        return !backingField.IsDefined(typeof(CompilerGeneratedAttribute), inherit: true) ? null : backingField;
    }
}

public class ReflectionHelper
{
    public static void SetPrivateReadonlyPropertyValue(object instance, string propertyName, object value)
    {
        var type = instance.GetType();
        var property = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);

        if (property == null)
            throw new ArgumentOutOfRangeException(nameof(propertyName), $@"Property {propertyName} was not found in Type {type.FullName}");

        var field = property.GetBackingField();
        field.SetValue(instance, value);
    }
}
