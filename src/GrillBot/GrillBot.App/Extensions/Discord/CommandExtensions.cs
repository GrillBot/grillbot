using Discord.Commands;
using GrillBot.Database.Enums;
using GrillBot.Database.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrillBot.App.Extensions.Discord
{
    static public class CommandExtensions
    {
        static private Dictionary<string, bool> CommandsStatus { get; set; }

        public static async Task InitializeCommandStatusCacheAsync(this CommandService _, IServiceProvider provider)
        {
            using var scope = provider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<GrillBotContext>();
            var commands = await context.Commands.ToListAsync();

            CommandsStatus = commands.ToDictionary(o => o.Name, o => !o.HaveFlags(CommandFlags.Blocked));
        }

        static public bool IsCommandEnabled(this CommandInfo command)
        {
            var format = command.GetCommandFormat();
            return !CommandsStatus.ContainsKey(format) || CommandsStatus[format];
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
