using Microsoft.Extensions.Caching.Memory;

namespace GrillBot.App.Managers.DataResolve;

public class GuildResolver : BaseDataResolver<ulong, IGuild, Database.Entity.Guild, Data.Models.API.Guilds.Guild>
{
    public GuildResolver(IDiscordClient discordClient, GrillBotDatabaseBuilder databaseBuilder, IMemoryCache memoryCache)
        : base(discordClient, databaseBuilder, memoryCache)
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

    protected override Data.Models.API.Guilds.Guild Map(IGuild discordEntity)
    {
        return new Data.Models.API.Guilds.Guild
        {
            Id = discordEntity.Id.ToString(),
            Name = discordEntity.Name
        };
    }

    protected override Data.Models.API.Guilds.Guild Map(Database.Entity.Guild entity)
    {
        return new Data.Models.API.Guilds.Guild
        {
            Id = entity.Id,
            Name = entity.Name
        };
    }
}
