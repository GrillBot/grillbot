using Discord.Interactions;
using GrillBot.App.Infrastructure;
using GrillBot.App.Infrastructure.Preconditions.Interactions;

namespace GrillBot.App.Modules.Interactions;

[Group("guild", "Guild management")]
[RequireUserPerms]
[RequireBotPermission(GuildPermission.Administrator)]
public class GuildModule : InteractionsModuleBase
{
    public GuildModule(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    [SlashCommand("info", "Guild information")]
    public async Task GetInfoAsync()
    {
        using var command = GetCommand<Actions.Commands.Guild.GuildInfo>();

        var result = await command.Command.ProcessAsync();
        await SetResponseAsync(embed: result);
    }
}
