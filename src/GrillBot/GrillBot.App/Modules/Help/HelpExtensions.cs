using Discord;
using Discord.Commands;
using GrillBot.App.Extensions;
using GrillBot.App.Extensions.Discord;
using GrillBot.App.Infrastructure.Embeds;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace GrillBot.App.Modules.Help
{
    static public class HelpExtensions
    {
        static public async Task<EmbedBuilder> WithHelpModuleAsync(this EmbedBuilder embed, ModuleInfo module, ICommandContext context, IServiceProvider provider, int pagesCount, string prefix,
            int page = 0)
        {
            embed
                .WithTitle(module.Name)
                .WithColor(Color.Blue)
                .WithCurrentTimestamp()
                .WithAuthor(o => o.WithName("Nápověda").WithIconUrl(context.Client.CurrentUser.GetAvatarUri()))
                .WithFooter($"{page + 1}/{pagesCount}")
                .WithMetadata(new HelpMetadata() { Page = page, PagesCount = pagesCount });

            if (!string.IsNullOrEmpty(module.Summary))
                embed.WithDescription(module.Summary);

            var executableCommands = await module.Commands
                .FindAllAsync(async cmd => (await cmd.CheckPreconditionsAsync(context, provider)).IsSuccess);
            foreach (var command in executableCommands.Take(EmbedBuilder.MaxFieldCount))
            {
                var summary = string.IsNullOrEmpty(command.Summary) ? "*Tento příkaz nemá popis*" : command.Summary.Replace("{prefix}", prefix);

                var aliases = command.GetAliasesFormat(prefix);
                if (!string.IsNullOrEmpty(aliases))
                    aliases = $"**Alias: ** *{aliases}*\n";

                embed.AddField(command.GetCommandFormat(prefix), aliases + summary);
            }

            return embed;
        }
    }
}
