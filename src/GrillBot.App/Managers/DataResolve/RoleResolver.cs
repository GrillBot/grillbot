using GrillBot.Cache.Services.Managers;

namespace GrillBot.App.Managers.DataResolve;

public class RoleResolver : BaseDataResolver<IRole, object, Data.Models.API.Role>
{
    public RoleResolver(IDiscordClient discordClient, GrillBotDatabaseBuilder databaseBuilder, DataCacheManager cache)
        : base(discordClient, databaseBuilder, cache)
    {
    }

    public Task<Data.Models.API.Role?> GetRoleAsync(ulong roleId)
    {
        return GetMappedEntityAsync(
            $"Role({roleId})",
            async () => (await _discordClient.GetGuildsAsync(CacheMode.CacheOnly)).Select(o => o.GetRole(roleId)).FirstOrDefault(o => o is not null),
            _ => Task.FromResult<object?>(null)
        );
    }

    protected override Data.Models.API.Role Map(IRole discordEntity)
    {
        return new Data.Models.API.Role
        {
            Name = discordEntity.Name,
            Id = discordEntity.Id.ToString(),
            Color = discordEntity.Color.ToString()
        };
    }

    protected override Data.Models.API.Role Map(object entity) => new();
}
