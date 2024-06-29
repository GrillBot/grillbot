namespace GrillBot.Common.Helpers;

public static class EnumHelper
{
    public static TEnum AggregateFlags<TEnum>() where TEnum : Enum
    {
        return (TEnum)(object)typeof(TEnum)
           .GetEnumValues()
           .Cast<int>()
           .Aggregate((prev, curr) => prev | curr);
    }
}
