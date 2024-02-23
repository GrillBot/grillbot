using AutoMapper;

namespace GrillBot.App.Managers.DataResolve;

public class RoleResolver : BaseDataResolver<ulong, IRole, object, Data.Models.API.Role>
{
    public RoleResolver(IDiscordClient discordClient, IMapper mapper, GrillBotDatabaseBuilder databaseBuilder)
        : base(discordClient, mapper, databaseBuilder)
    {
    }

    public Task<Data.Models.API.Role?> GetRoleAsync(ulong roleId)
    {
        return GetMappedEntityAsync(
            roleId,
            async () => (await DiscordClient.GetGuildsAsync(CacheMode.CacheOnly)).Select(o => o.GetRole(roleId)).FirstOrDefault(o => o is not null),
            _ => Task.FromResult<object?>(null)
        );
    }
}
