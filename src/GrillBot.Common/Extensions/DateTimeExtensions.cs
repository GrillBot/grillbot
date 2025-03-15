using Discord;

namespace GrillBot.Common.Extensions;

public static class DateTimeExtensions
{
    public static string ToCzechFormat(this DateTime dateTime, bool withoutTime = false, bool withMiliseconds = false)
    {
        var timeFormat = " HH:mm:ss" + (withMiliseconds ? ".ffff" : "");
        return dateTime.ToString($"dd. MM. yyyy{(withoutTime ? "" : timeFormat)}");
    }

    public static string ToCzechFormat(this DateOnly dateOnly)
        => dateOnly.ToDateTime(new TimeOnly(0, 0, 0)).ToCzechFormat(true);

    public static string ToTimestampMention(this DateTimeOffset dateTime)
        => TimestampTag.FromDateTimeOffset(dateTime).ToString(TimestampTagStyles.ShortDateTime);

    public static string ToTimestampMention(this DateTime dateTime)
        => TimestampTag.FromDateTime(dateTime).ToString(TimestampTagStyles.ShortDateTime);
}
