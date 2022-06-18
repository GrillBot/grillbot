namespace GrillBot.Common.Extensions;

public static class StringExtensions
{
    public static string? Cut(this string? str, int maxLength, bool withoutDots = false)
    {
        if (str == null) return null;

        var withoutDotsLen = withoutDots ? 0 : 3;
        if (str.Length >= maxLength - withoutDotsLen)
            str = str[..(maxLength - withoutDotsLen)] + (withoutDots ? "" : "...");

        return str;
    }
}
