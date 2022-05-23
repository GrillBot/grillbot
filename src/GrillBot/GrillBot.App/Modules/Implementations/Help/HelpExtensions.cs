#pragma warning disable S1075 // URIs should not be hardcoded
using Discord.Commands;
using GrillBot.App.Infrastructure.Embeds;
using GrillBot.App.Helpers;
using GrillBot.Data.Extensions;
using GrillBot.Common.Extensions.Discord;

namespace GrillBot.App.Modules.Implementations.Help;

static public class HelpExtensions
{
    static public async Task<EmbedBuilder> WithHelpModuleAsync(this EmbedBuilder embed, ModuleInfo module, ICommandContext context, IServiceProvider provider, int pagesCount, string prefix,
        int page = 0)
    {
        embed
            .WithTitle(module.Name)
            .WithColor(Color.Blue)
            .WithCurrentTimestamp()
            .WithAuthor(o => o.WithName("Nápověda").WithIconUrl(context.Client.CurrentUser.GetUserAvatarUrl()).WithUrl("https://public.grillbot.cloud"))
            .WithFooter($"{page + 1}/{pagesCount}")
            .WithMetadata(new HelpMetadata() { Page = page, PagesCount = pagesCount });

        const string summaryTitle = "Kompletní seznam lze také najít ve veřejné administraci bota (https://public.grillbot.cloud). **Pokud některé příkazy nevidíte, tak je zkuste hledat jako příkaz s prefixem `/`**";
        if (!string.IsNullOrEmpty(module.Summary))
            embed.WithDescription(summaryTitle + "\n" + module.Summary);
        else
            embed.WithDescription(summaryTitle);

        var executableCommands = await module.Commands
            .FindAllAsync(async cmd => (await cmd.CheckPreconditionsAsync(context, provider)).IsSuccess);
        foreach (var command in executableCommands.Take(EmbedBuilder.MaxFieldCount - 1))
        {
            var summary = string.IsNullOrEmpty(command.Summary) ? "*Tento příkaz nemá popis*" : FormatHelper.FormatCommandDescription(command.Summary, prefix);

            var aliases = command.GetAliasesFormat(prefix);
            if (!string.IsNullOrEmpty(aliases))
                aliases = $"**Alias: ** *{aliases}*\n";

            embed.AddField(command.GetCommandFormat(prefix), aliases + summary);
        }

        return embed;
    }
}
