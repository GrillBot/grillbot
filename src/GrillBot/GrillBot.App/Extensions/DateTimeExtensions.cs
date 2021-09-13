using System;

namespace GrillBot.App.Extensions
{
    static public class DateTimeExtensions
    {
        static public string ToCzechFormat(this DateTime dateTime, bool withoutTime = false)
        {
            return dateTime.ToString($"dd. MM. yyyy{(withoutTime ? "" : " HH:mm:ss")}");
        }

        static public int ComputeAge(this DateTime dateTime)
        {
            var today = DateTime.Today;
            var age = today.Year - dateTime.Year;
            if (dateTime.Date > today.AddYears(-age)) age--;

            return age;
        }
    }
}
