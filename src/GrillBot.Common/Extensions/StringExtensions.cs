namespace GrillBot.Common.Extensions;

public static class StringExtensions
{
    public static string ReplaceIfNullOrEmpty(this string? value, string replacement)
        => string.IsNullOrEmpty(value) ? replacement : value;

    public static string RemoveInvalidUnicodeChars(this string value)
        => value.Replace('\uD83C', '\0').Replace('\uDC73', '\0').Replace('\uD83E', '\0');
}
