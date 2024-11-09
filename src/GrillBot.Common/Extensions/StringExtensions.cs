using System.Text;

namespace GrillBot.Common.Extensions;

public static class StringExtensions
{
    public static string ReplaceIfNullOrEmpty(this string? value, string replacement)
        => string.IsNullOrEmpty(value) ? replacement : value;

    public static string RemoveInvalidUnicodeChars(this string value)
    {
        var sb = new StringBuilder();

        foreach (var character in value.Where(ch => Rune.TryCreate(ch, out _)))
            sb.Append(character);

        return sb.ToString();
    }
}
