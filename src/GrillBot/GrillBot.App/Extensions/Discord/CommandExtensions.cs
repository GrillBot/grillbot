using Discord.Commands;
using GrillBot.App.Helpers;

namespace GrillBot.App.Extensions.Discord
{
    static public class CommandExtensions
    {
        static public IEnumerable<string> GetAliases(this CommandInfo command, string prefix)
            => command.Aliases.Skip(1).Select(a => prefix + a);

        static public string GetAliasesFormat(this CommandInfo command, string prefix)
            => string.Join(", ", GetAliases(command, prefix));

        // Credits to Janch
        static public string GetCommandFormat(this CommandInfo command, string prefix = null)
        {
            var builder = new StringBuilder();
            if (!string.IsNullOrEmpty(prefix)) builder.Append(prefix);

            builder.Append(command.Aliases[0]);

            foreach (var param in command.Parameters.Where(o => !string.IsNullOrEmpty(o.Name)))
            {
                builder
                    .Append(" `")
                    .Append(FormatHelper.FormatParameter(param.Name, param.IsOptional))
                    .Append('`');
            }

            return builder.ToString();
        }
    }
}
