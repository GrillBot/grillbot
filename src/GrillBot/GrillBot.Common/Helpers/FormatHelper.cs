using System.Globalization;
using GrillBot.Common.Managers;
using GrillBot.Common.Managers.Localization;
using Humanizer;
using Markdig;

namespace GrillBot.Common.Helpers;

public class FormatHelper
{
    private ITextsManager Texts { get; }

    public FormatHelper(ITextsManager texts)
    {
        Texts = texts;
    }

    public static string FormatMembersToCzech(long count) => Format(count, "člen", "členové", "členů");
    public static string FormatBooleanToCzech(bool val) => val ? "Ano" : "Ne";
    public static string FormatMessagesToCzech(long count) => Format(count, "zpráva", "zprávy", "zpráv");
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

    public string FormatNumber(string id, string locale, long count)
    {
        var countId = count switch
        {
            1 => "One",
            > 1 and < 5 => "TwoToFour",
            _ => "FiveAndMore"
        };

        var text = Texts[$"{id}/{countId}", locale];
        return text.FormatWith(Texts.GetCulture(locale), count);
    }

    public string FormatBoolean(string id, string locale, bool value)
        => Texts[$"{id}/{value}", locale];
}
