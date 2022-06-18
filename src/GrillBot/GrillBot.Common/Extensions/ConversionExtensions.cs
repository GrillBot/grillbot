namespace GrillBot.Common.Extensions;

public static class ConversionExtensions
{
    public static ulong ToUlong(this string str) => string.IsNullOrEmpty(str) ? default : Convert.ToUInt64(str);
    public static int ToInt(this string str) => Convert.ToInt32(str);
}
