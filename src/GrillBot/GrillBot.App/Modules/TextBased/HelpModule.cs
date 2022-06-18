﻿using Discord.Commands;
using GrillBot.App.Infrastructure.Preconditions.TextBased;
using GrillBot.App.Modules.Implementations.Help;
using GrillBot.Common;
using GrillBot.Common.Extensions;

namespace GrillBot.App.Modules.TextBased;

[Name("Nápověda")]
[RequireUserPerms]
public class HelpModule : Infrastructure.ModuleBase
{
    private CommandService CommandService { get; }
    private IServiceProvider Provider { get; }
    private string CommandPrefix { get; }

    public HelpModule(CommandService commandService, IServiceProvider provider, IConfiguration configuration)
    {
        CommandService = commandService;
        Provider = provider;

        CommandPrefix = configuration.GetValue<string>("Discord:Commands:Prefix");
    }

    [Command("help")]
    [Summary("Zobrazí nápovědu.")]
    public Task HelpAsync() => HelpAsync(null);

    [Command("help")]
    [Summary("Zobrazí nápovědu pro zadaný příkaz.")]
    public async Task HelpAsync([Remainder][Name("prikaz")] string query)
    {
        var availableModules = await CommandService.Modules
            .Where(o => o.Commands.Count > 0)
            .FindAllAsync(async mod => (await mod.GetExecutableCommandsAsync(Context, Provider)).Count > 0);

        var module = availableModules.FirstOrDefault();
        if (module == null)
        {
            await ReplyAsync("je mi to líto, ale nemáš k dispozici žádné příkazy.");
            return;
        }

        if (!string.IsNullOrEmpty(query))
        {
            var foundModule = availableModules.Find(m => m.Commands.Any(c => c.Aliases.Any(a => a.Contains(query))));
            if (foundModule == null)
            {
                var commandSearch = CommandService.Search(query);
                if (commandSearch.IsSuccess)
                    foundModule = commandSearch.Commands[0].Command.Module;
            }

            if (foundModule != null)
                module = foundModule;
        }

        var embed = await new EmbedBuilder().WithHelpModuleAsync(module, Context, Provider, availableModules.Count, CommandPrefix, availableModules.IndexOf(module));
        var message = await ReplyAsync(embed: embed.Build());
        await message.AddReactionsAsync(Emojis.PaginationEmojis);
    }
}
