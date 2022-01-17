using Humanizer;
using System.Globalization;

namespace GrillBot.Data.Helpers
{
    static public class FormatHelper
    {
        static public string FormatMembersToCzech(long count) => Format(count, "člen", "členové", "členů");
        static public string FormatBooleanToCzech(bool val) => val ? "Ano" : "Ne";
        static public string FormatMessagesToCzech(long count) => Format(count, "zpráva", "zprávy", "zpráv");
        static public string FormatPermissionstoCzech(long count) => Format(count, "oprávnění", "oprávnění", "oprávnění");
        static public string FormatPointsToCzech(long count) => Format(count, "bod", "body", "bodů");

        static private string Format(long count, string oneSuffix, string twoToFour, string fiveAndMore)
        {
            if (count == 1) return $"1 {oneSuffix}";
            else if (count > 1 && count < 5) return $"{count} {twoToFour}";
            else return $"{"{0:N0}".FormatWith(new CultureInfo("cs-CZ"), count)} {fiveAndMore}";
        }
    }
}
