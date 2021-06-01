namespace GrillBot.App.Extensions
{
    static public class StringExtensions
    {
        static public string Cut(this string str, int maxLength, bool withoutDots = false)
        {
            if (str == null) return null;

            var withoutDotsLen = withoutDots ? 0 : 3;
            if (str.Length >= maxLength - withoutDotsLen)
                str = str.Substring(0, maxLength - withoutDotsLen) + (withoutDots ? "" : "...");

            return str;
        }
    }
}
