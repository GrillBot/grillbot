using GrillBot.Common.Managers.Localization;
using Humanizer;

namespace GrillBot.Common.Helpers;

public class FormatHelper
{
    private ITextsManager Texts { get; }

    public FormatHelper(ITextsManager texts)
    {
        Texts = texts;
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
