using AutoMapper;

namespace GrillBot.App.Managers.DataResolve;

public class GuildResolver : BaseDataResolver<ulong, IGuild, Database.Entity.Guild, Data.Models.API.Guilds.Guild>
{
    public GuildResolver(IDiscordClient discordClient, IMapper mapper, GrillBotDatabaseBuilder databaseBuilder)
        : base(discordClient, mapper, databaseBuilder)
    {
    }

    public Task<Data.Models.API.Guilds.Guild?> GetGuildAsync(ulong guildId)
    {
        return GetMappedEntityAsync(
            guildId,
            () => _discordClient.GetGuildAsync(guildId, CacheMode.CacheOnly),
            repo => repo.Guild.FindGuildByIdAsync(guildId, true)
        );
    }
}
