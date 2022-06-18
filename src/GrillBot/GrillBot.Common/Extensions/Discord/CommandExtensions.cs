using System.Text;
using Discord.Commands;

namespace GrillBot.Common.Extensions.Discord;

public static class CommandExtensions
{
    public static IEnumerable<string> GetAliases(this CommandInfo command, string prefix)
        => command.Aliases.Skip(1).Select(a => prefix + a);

    public static string GetAliasesFormat(this CommandInfo command, string prefix)
        => string.Join(", ", GetAliases(command, prefix));

    // Credits to Janch
    public static string GetCommandFormat(this CommandInfo command, string? prefix = null)
    {
        var builder = new StringBuilder();
        if (!string.IsNullOrEmpty(prefix)) builder.Append(prefix);

        builder.Append(command.Aliases[0]);

        foreach (var param in command.Parameters.Where(o => !string.IsNullOrEmpty(o.Name)))
        {
            builder
                .Append(" `")
                .Append(FormatParameter(param))
                .Append('`');
        }

        return builder.ToString();
    }

    public static string? FormatParameter(this ParameterInfo? info)
        => info == null ? null : (info.IsOptional ? "[" : "") + info.Name + (info.IsOptional ? "]" : "");
}
