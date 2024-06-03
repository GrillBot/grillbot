namespace GrillBot.Common.Extensions;

public static class StringExtensions
{
    public static string ReplaceIfNullOrEmpty(this string? value, string replacement)
        => string.IsNullOrEmpty(value) ? replacement : value;
}
