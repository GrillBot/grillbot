namespace GrillBot.Common.Extensions;

public static class DateTimeExtensions
{
    public static string ToCzechFormat(this DateTime dateTime, bool withoutTime = false, bool withMiliseconds = false)
    {
        var timeFormat = $" HH:mm:ss" + (withMiliseconds ? ".ffff" : "");
        return dateTime.ToString($"dd. MM. yyyy{(withoutTime ? "" : timeFormat)}");
    }

    public static int ComputeAge(this DateTime dateTime)
    {
        var today = DateTime.Today;
        var age = today.Year - dateTime.Year;
        if (dateTime.Date > today.AddYears(-age)) age--;

        return age;
    }
}
