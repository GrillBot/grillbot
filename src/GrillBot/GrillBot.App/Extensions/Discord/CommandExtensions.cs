using Discord.Commands;
using System.Linq;
using System.Text;

namespace GrillBot.Data.Extensions.Discord
{
    static public class CommandExtensions
    {
        static public string GetAliasesFormat(this CommandInfo command, string prefix)
        {
            var aliases = command.Aliases.Skip(1).Select(a => prefix + a);
            return string.Join(", ", aliases);
        }

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
                    .Append(param.IsOptional ? "[" : "").Append(param.Name).Append(param.IsOptional ? "]" : "")
                    .Append('`');
            }

            return builder.ToString();
        }
    }
}
