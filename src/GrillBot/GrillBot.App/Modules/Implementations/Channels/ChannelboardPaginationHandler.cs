using GrillBot.App.Helpers;
using GrillBot.App.Infrastructure;

namespace GrillBot.App.Modules.Implementations.Channels;

public class ChannelboardPaginationHandler : ComponentInteractionHandler
{
    private IDiscordClient DiscordClient { get; }
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private int Page { get; }

    public ChannelboardPaginationHandler(IDiscordClient discordClient, GrillBotDatabaseBuilder databaseBuilder, int page)
    {
        DiscordClient = discordClient;
        DatabaseBuilder = databaseBuilder;
        Page = page;
    }

    public override async Task ProcessAsync(IInteractionContext context)
    {
        if (!TryParseData<ChannelboardMetadata>(context.Interaction, out var component, out var metadata))
        {
            await context.Interaction.DeferAsync();
            return;
        }

        var guild = await DiscordClient.GetGuildAsync(metadata.GuildId);
        if (guild == null)
        {
            await context.Interaction.DeferAsync();
            return;
        }

        var user = context.User is IGuildUser guildUser ? guildUser : await DiscordClient.TryFindGuildUserAsync(guild.Id, context.User.Id);
        var availableChannels = await guild.GetAvailableChannelsAsync(user, true);
        if (availableChannels.Count == 0)
        {
            await context.Interaction.DeferAsync();
            return;
        }

        var availableChannelIds = availableChannels.ConvertAll(o => o.Id.ToString());

        using var repository = DatabaseBuilder.CreateRepository();
        var channels = await repository.Channel.GetVisibleChannelsAsync(guild.Id, availableChannelIds, true);

        if (channels.Count == 0)
        {
            await context.Interaction.DeferAsync();
            return;
        }

        var pagesCount = (int)Math.Ceiling(channels.Count / 10.0D);
        var newPage = CheckNewPageNumber(Page, pagesCount);
        if (newPage == metadata.Page)
        {
            await context.Interaction.DeferAsync();
            return;
        }

        var skip = newPage * 10;
        var data = channels
            .Select(o => new { o.ChannelId, Count = o.Users.Sum(x => x.Count) })
            .OrderByDescending(o => o.Count)
            .Skip(skip)
            .Take(10)
            .ToDictionary(o => o.ChannelId, o => o.Count);

        var embed = new EmbedBuilder()
            .WithChannelboard(user, guild, data, id => guild.GetTextChannelAsync(id).Result, skip, newPage);

        await component.UpdateAsync(msg =>
        {
            msg.Components = ComponentsHelper.CreatePaginationComponents(newPage, pagesCount, "channelboard");
            msg.Embed = embed.Build();
        });
    }
}
