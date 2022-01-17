using System;
using System.Text.RegularExpressions;

namespace GrillBot.Data.Extensions
{
    static public class StringExtensions
    {
        static public string Cut(this string str, int maxLength, bool withoutDots = false)
        {
            if (str == null) return null;

            var withoutDotsLen = withoutDots ? 0 : 3;
            if (str.Length >= maxLength - withoutDotsLen)
                str = str[..(maxLength - withoutDotsLen)] + (withoutDots ? "" : "...");

            return str;
        }

        /// <summary>
        /// Parses time from string.
        /// </summary>
        /// <param name="str">Some string</param>
        /// <param name="fromAny">Find time on any position of string.</param>
        static public TimeSpan? ParseTime(this string str, bool fromAny = false)
        {
            var regex = @"(\d*):(\d*):?(\d*)?";
            if (!fromAny) regex = $"^{regex}";

            var match = Regex.Match(str, regex);
            if (match.Success)
            {
                var hours = Convert.ToInt32(match.Groups[1].Value);
                var minutes = Convert.ToInt32(match.Groups[2].Value);
                var seconds = string.IsNullOrEmpty(match.Groups[3].Value) ? 0 : Convert.ToInt32(match.Groups[3].Value);

                return new TimeSpan(hours, minutes, seconds);
            }

            return null;
        }
    }
}
