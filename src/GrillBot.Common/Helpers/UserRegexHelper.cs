using System.Text.RegularExpressions;

namespace GrillBot.Common.Helpers;

public static partial class UserRegexHelper
{
    [GeneratedRegex("^(me|j[a|á])$", RegexOptions.IgnoreCase, "cs-CZ")]
    public static partial Regex IsMeRegex();

    [GeneratedRegex("<@(\\d+)>")]
    public static partial Regex UserIdRegex();
}
