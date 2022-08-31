using System.Globalization;
using Humanizer;
using Markdig;

namespace GrillBot.Common.Helpers;

public static class FormatHelper
{
    public static string FormatMembersToCzech(long count) => Format(count, "člen", "členové", "členů");
    public static string FormatBooleanToCzech(bool val) => val ? "Ano" : "Ne";
    public static string FormatMessagesToCzech(long count) => Format(count, "zpráva", "zprávy", "zpráv");
    public static string FormatPermissionstoCzech(long count) => Format(count, "oprávnění", "oprávnění", "oprávnění");
    public static string FormatPointsToCzech(long count) => Format(count, "bod", "body", "bodů");

    public static string Format(long count, string oneSuffix, string twoToFour, string fiveAndMore)
    {
        return count switch
        {
            1 => $"1 {oneSuffix}",
            > 1 and < 5 => $"{count} {twoToFour}",
            _ => $"{"{0:N0}".FormatWith(new CultureInfo("cs-CZ"), count)} {fiveAndMore}"
        };
    }

    public static string? FormatCommandDescription(string description, string prefix, bool toHtml = false)
    {
        if (string.IsNullOrEmpty(description)) return null;

        description = description.Trim().Replace("{prefix}", prefix);
        description = description.Replace("<", "&lt;").Replace(">", "&gt;");

        return toHtml ? Markdown.ToHtml(description).Replace("\n", " ") : description;
    }
}
