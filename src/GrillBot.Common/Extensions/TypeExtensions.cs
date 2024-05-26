using System.Reflection;

namespace GrillBot.Common.Extensions;

public static class TypeExtensions
{
    public static IEnumerable<FieldInfo> GetConstants(this Type type)
    {
        return type
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            .Where(f => f.IsLiteral && !f.IsInitOnly);
    }
}
