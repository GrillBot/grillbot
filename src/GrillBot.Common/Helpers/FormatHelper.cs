using GrillBot.Common.Managers.Localization;

namespace GrillBot.Common.Helpers;

public class FormatHelper(ITextsManager _texts)
{
    public string FormatNumber(string id, string locale, long count)
    {
        var countId = count switch
        {
            1 => "One",
            > 1 and < 5 => "TwoToFour",
            _ => "FiveAndMore"
        };

        var text = _texts[$"{id}/{countId}", locale];
        return string.Format(_texts.GetCulture(locale), text, count);
    }

    public string FormatBoolean(string id, string locale, bool value)
        => _texts[$"{id}/{value}", locale];
}
