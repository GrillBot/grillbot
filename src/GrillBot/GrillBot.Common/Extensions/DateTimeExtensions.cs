namespace GrillBot.Common.Extensions;

public static class DateTimeExtensions
{
    public static string ToCzechFormat(this DateTime dateTime, bool withoutTime = false)
    {
        return dateTime.ToString($"dd. MM. yyyy{(withoutTime ? "" : " HH:mm:ss")}");
    }

    public static int ComputeAge(this DateTime dateTime)
    {
        var today = DateTime.Today;
        var age = today.Year - dateTime.Year;
        if (dateTime.Date > today.AddYears(-age)) age--;

        return age;
    }
}
