namespace GrillBot.App.Helpers
{
    static public class FormatHelper
    {
        static public string FormatMembersToCzech(long count)
        {
            if (count == 1) return "1 člen";
            else if (count > 1 && count < 5) return $"{count} členové";
            else return $"{count} členů";
        }

        static public string FormatBooleanToCzech(bool val)
        {
            return val ? "Ano" : "Ne";
        }
    }
}
