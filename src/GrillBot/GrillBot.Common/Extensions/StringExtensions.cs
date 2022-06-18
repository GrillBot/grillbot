using System.Text.RegularExpressions;

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

    /// <summary>
    /// Parses time from string.
    /// </summary>
    /// <param name="str">Some string</param>
    /// <param name="fromAny">Find time on any position of string.</param>
    public static TimeSpan? ParseTime(this string str, bool fromAny = false)
    {
        var regex = @"(\d*):(\d*):?(\d*)?";
        if (!fromAny) regex = $"^{regex}";

        var match = Regex.Match(str, regex);
        if (!match.Success)
            return null;

        var hours = match.Groups[1].Value.ToInt();
        var minutes = match.Groups[2].Value.ToInt();
        var seconds = string.IsNullOrEmpty(match.Groups[3].Value) ? 0 : match.Groups[3].Value.ToInt();

        return new TimeSpan(hours, minutes, seconds);
    }
}
