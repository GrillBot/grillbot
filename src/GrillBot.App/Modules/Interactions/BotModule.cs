using Discord.Interactions;
using GrillBot.App.Infrastructure;
using GrillBot.App.Infrastructure.Preconditions.Interactions;
using GrillBot.Core.Exceptions;

namespace GrillBot.App.Modules.Interactions;

[RequireUserPerms]
[Group("bot", "Bot information and configuration commands.")]
public class BotModule : InteractionsModuleBase
{
    public BotModule(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    [SlashCommand("info", "Bot info")]
    public async Task BotInfoAsync()
    {
        using var command = GetCommand<Actions.Commands.BotInfo>();
        var embed = await command.Command.ProcessAsync();

        await SetResponseAsync(embed: embed);
    }

    [Group("selfunverify", "Configuring selfunverify.")]
    public class SelfUnverifyConfig : InteractionsModuleBase
    {
        public SelfUnverifyConfig(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        [SlashCommand("list-keepables", "List of allowable accesses when selfunverify")]
        public async Task ListAsync(string? group = null)
        {
            using var command = GetCommand<Actions.Commands.Unverify.SelfUnverifyKeepables>();

            try
            {
                var embed = await command.Command.ListAsync(group);
                await SetResponseAsync(embed: embed);
            }
            catch (NotFoundException ex)
            {
                await SetResponseAsync(ex.Message);
            }
        }
    }
}
