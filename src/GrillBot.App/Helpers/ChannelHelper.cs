using GrillBot.Core.Extensions;

namespace GrillBot.App.Helpers;

public class ChannelHelper
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private IDiscordClient DiscordClient { get; }

    public ChannelHelper(GrillBotDatabaseBuilder databaseBuilder, IDiscordClient discordClient)
    {
        DatabaseBuilder = databaseBuilder;
        DiscordClient = discordClient;
    }

    /// <summary>
    /// Tries find guild from channel. If channel is DM method will return null;
    /// If channel is null and channelId is filled (typical usage for <see cref="Cacheable{TEntity, TId}"/>) method tries find guild with database data.
    /// </summary>
    public async Task<IGuild?> GetGuildFromChannelAsync(IChannel? channel, ulong channelId)
    {
        switch (channel)
        {
            case IDMChannel:
                return null; // Direct messages
            case IGuildChannel guildChannel:
                return guildChannel.Guild;
            case null when channelId == default:
                return null;
        }

        using var repository = DatabaseBuilder.CreateRepository();

        var channelEntity = await repository.Channel.FindChannelByIdAsync(channelId, null, true, includeDeleted: true);
        if (channelEntity == null)
            return null;

        var guildId = channelEntity.GuildId.ToUlong();
        return await DiscordClient.GetGuildAsync(guildId);
    }
}
