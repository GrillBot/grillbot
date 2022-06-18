using System.ComponentModel;
using System.Reflection;

namespace GrillBot.Common.Extensions;

public static class EnumExtensions
{
    public static string? GetDescription(this Enum @enum) => GetAttribute<DescriptionAttribute>(@enum)?.Description;

    private static TAttribute? GetAttribute<TAttribute>(this Enum @enum) where TAttribute : Attribute
    {
        var member = @enum.GetType().GetMember(@enum.ToString());
        return member[0].GetCustomAttribute<TAttribute>(false);
    }
}
