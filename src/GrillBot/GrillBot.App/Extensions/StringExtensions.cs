namespace GrillBot.App.Extensions
{
    static public class StringExtensions
    {
        static public string Cut(this string str, int maxLength)
        {
            if (str == null) return null;

            if (str.Length >= maxLength - 3)
                str = str.Substring(0, maxLength - 3) + "...";

            return str;
        }
    }
}
