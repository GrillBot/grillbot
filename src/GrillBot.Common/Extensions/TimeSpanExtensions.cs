namespace GrillBot.Common.Extensions;

public static class TimeSpanExtensions
{
    public static long ToTotalMiliseconds(this TimeSpan timeSpan)
        => Convert.ToInt64(timeSpan.TotalMilliseconds);
}
