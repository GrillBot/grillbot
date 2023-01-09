using Humanizer;

namespace GrillBot.Common.Extensions;

public static class NumberExtensions
{
    public static string FormatNumber(this sbyte number) => FormatNumber(GetFormatTemplate<sbyte>(), number);
    public static string FormatNumber(this byte number) => FormatNumber(GetFormatTemplate<byte>(), number);
    public static string FormatNumber(this short number) => FormatNumber(GetFormatTemplate<short>(), number);
    public static string FormatNumber(this ushort number) => FormatNumber(GetFormatTemplate<ushort>(), number);
    public static string FormatNumber(this int number) => FormatNumber(GetFormatTemplate<int>(), number);
    public static string FormatNumber(this uint number) => FormatNumber(GetFormatTemplate<uint>(), number);
    public static string FormatNumber(this long number) => FormatNumber(GetFormatTemplate<long>(), number);
    public static string FormatNumber(this ulong number) => FormatNumber(GetFormatTemplate<ulong>(), number);

    private static string FormatNumber(string template, object number) => template.FormatWith(number).Trim();

    private static string GetFormatTemplate<T>()
    {
        var type = typeof(T);
        if (type == typeof(sbyte) || type == typeof(byte))
            return "{0:###}";
        if (type == typeof(short) || type == typeof(short))
            return "{0:## ###}";
        if (type == typeof(int) || type == typeof(uint))
            return "{0:# ### ### ###}";
        if (type == typeof(long))
            return "{0:# ### ### ### ### ### ###}";
        if (type == typeof(ulong))
            return "{0:## ### ### ### ### ### ###}";

        throw new ArgumentException("Unsupported number type");
    }
}
