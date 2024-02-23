using AutoMapper;

namespace GrillBot.App.Managers.DataResolve;

public class GuildDataResolver : BaseDataResolver<ulong, IGuild, Database.Entity.Guild, Data.Models.API.Guilds.Guild>
{
    public GuildDataResolver(IDiscordClient discordClient, GrillBotDatabaseBuilder databaseBuilder, IMapper mapper)
        : base(discordClient, mapper, databaseBuilder)
    {
    }

    public Task<Data.Models.API.Guilds.Guild?> GetGuildAsync(ulong guildId)
    {
        return GetMappedEntityAsync(
            guildId,
            () => DiscordClient.GetGuildAsync(guildId, CacheMode.CacheOnly),
            repo => repo.Guild.FindGuildByIdAsync(guildId, true)
        );
    }
}
