using System.Text.RegularExpressions;

namespace GrillBot.Common.Helpers;

public static partial class DateTimeRegexHelper
{
    private const RegexOptions Options = RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.CultureInvariant | RegexOptions.IgnorePatternWhitespace;

    [GeneratedRegex("^(today|dnes(ka)?)$", Options)]
    private static partial Regex Today();

    [GeneratedRegex("^(tommorow|z[ií]tra|za[jv]tra)$", Options)]
    private static partial Regex Tommorow();

    [GeneratedRegex("^(v[cč]era|yesterday|vchora)$", Options)]
    private static partial Regex Yesterday();

    [GeneratedRegex("^(poz[ií]t[rř][ií]|pozajtra|poslezavtra)$", Options)]
    private static partial Regex DayAfterTommorow();

    [GeneratedRegex("^(^(te[dď]|now|(te|za)raz)$)$", Options)]
    private static partial Regex Now();

    [GeneratedRegex(@"(\d+)(m|h|d|w|M|y|r)", Options)]
    public static partial Regex TimeShift();

    public static DateTime? TryConvert(string value)
    {
        if (Today().IsMatch(value))
            return DateTime.Today;
        if (Tommorow().IsMatch(value))
            return DateTime.Now.AddDays(1);
        if (Yesterday().IsMatch(value))
            return DateTime.Now.AddDays(-1);
        if (DayAfterTommorow().IsMatch(value))
            return DateTime.Now.AddDays(2);
        if (Now().IsMatch(value))
            return DateTime.Now;

        return null;
    }
}
