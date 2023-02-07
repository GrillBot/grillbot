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

    public static string? FormatCommandDescription(string? description, string prefix, bool toHtml = false)
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
