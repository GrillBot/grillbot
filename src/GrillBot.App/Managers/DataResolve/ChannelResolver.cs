using GrillBot.Data.Models.API.Channels;
using GrillBot.Database.Entity;
using Microsoft.Extensions.Caching.Memory;

namespace GrillBot.App.Managers.DataResolve;

public class ChannelResolver : BaseDataResolver<Tuple<ulong, ulong>, IGuildChannel, GuildChannel, Channel>
{
    public ChannelResolver(IDiscordClient discordClient, GrillBotDatabaseBuilder databaseBuilder, IMemoryCache memoryCache)
        : base(discordClient, databaseBuilder, memoryCache)
    {
    }

    public Task<Channel?> GetChannelAsync(ulong guildId, ulong channelId)
    {
        return GetMappedEntityAsync(
            Tuple.Create(guildId, channelId),
            async () =>
            {
                var guild = await _discordClient.GetGuildAsync(guildId, CacheMode.CacheOnly);
                return guild is null ? null : await guild.GetChannelAsync(channelId, CacheMode.CacheOnly);
            },
            repo => repo.Channel.FindChannelByIdAsync(channelId, guildId, true, includeDeleted: true)
        );
    }

    protected override Channel Map(IGuildChannel discordEntity)
    {
        return new Channel
        {
            Id = discordEntity.Id.ToString(),
            Name = discordEntity.Name,
            Type = discordEntity.GetChannelType()
        };
    }

    protected override Channel Map(GuildChannel entity)
    {
        return new Channel
        {
            Type = entity.ChannelType,
            Name = entity.Name,
            Id = entity.ChannelId
        };
    }
}
