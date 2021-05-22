using System;

namespace GrillBot.App.Extensions
{
    static public class DateTimeExtensions
    {
        static public string ToCzechFormat(this DateTime dateTime, bool withoutTime = false)
        {
            return dateTime.ToString($"dd. MM. yyyy{(withoutTime ? "" : " HH:mm:ss")}");
        }
    }
}
