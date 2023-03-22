using Discord.Interactions;
using GrillBot.App.Infrastructure.Commands;
using GrillBot.App.Infrastructure.Preconditions.Interactions;
using GrillBot.App.Modules.Implementations.Channels;
using GrillBot.Core.Exceptions;

namespace GrillBot.App.Modules.Interactions;

[Group("channel", "Channel information")]
[RequireUserPerms]
public class ChannelModule : InteractionsModuleBase
{
    public ChannelModule(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    [SlashCommand("info", "Channel information")]
    public async Task GetChannelInfoAsync(
        SocketGuildChannel channel,
        [Choice("Hide threads", "true")] [Choice("Show threads", "false")]
        bool excludeThreads = false
    )
    {
        using var command = GetCommand<Actions.Commands.ChannelInfo>();
        var embed = await command.Command.ProcessAsync(channel, excludeThreads);

        if (command.Command.IsOk)
            await SetResponseAsync(embed: embed);
        else
            await SetResponseAsync(command.Command.ErrorMessage);
    }

    [SlashCommand("board", "TOP 10 channel statistics you can access.")]
    public async Task GetChannelboardAsync()
    {
        using var command = GetCommand<Actions.Commands.GetChannelboard>();

        try
        {
            var (embed, paginationComponents) = await command.Command.ProcessAsync(0);
            await SetResponseAsync(embed: embed, components: paginationComponents);
        }
        catch (NotFoundException ex)
        {
            await SetResponseAsync(ex.Message);
        }
    }

    [RequireSameUserAsAuthor]
    [ComponentInteraction("channelboard:*", ignoreGroupNames: true)]
    public async Task HandleChannelboardPaginationAsync(int page)
    {
        var handler = new ChannelboardPaginationHandler(ServiceProvider, page);
        await handler.ProcessAsync(Context);
    }
}
