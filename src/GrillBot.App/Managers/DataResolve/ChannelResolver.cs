using AutoMapper;

namespace GrillBot.App.Managers.DataResolve;

public class ChannelResolver : BaseDataResolver<Tuple<ulong, ulong>, IGuildChannel, Database.Entity.GuildChannel, Data.Models.API.Channels.Channel>
{
    public ChannelResolver(IDiscordClient discordClient, IMapper mapper, GrillBotDatabaseBuilder databaseBuilder)
        : base(discordClient, mapper, databaseBuilder)
    {
    }

    public Task<Data.Models.API.Channels.Channel?> GetChannelAsync(ulong guildId, ulong channelId)
    {
        return GetMappedEntityAsync(
            Tuple.Create(guildId, channelId),
            async () =>
            {
                var guild = await DiscordClient.GetGuildAsync(guildId, CacheMode.CacheOnly);
                return guild is null ? null : await guild.GetChannelAsync(channelId, CacheMode.CacheOnly);
            },
            repo => repo.Channel.FindChannelByIdAsync(channelId, guildId, true)
        );
    }
}
