using AutoMapper;

namespace GrillBot.App.Managers.DataResolve;

public class GuildUserResolver : BaseDataResolver<Tuple<ulong, ulong>, IGuildUser, Database.Entity.GuildUser, Data.Models.API.Users.GuildUser>
{
    public GuildUserResolver(IDiscordClient discordClient, IMapper mapper, GrillBotDatabaseBuilder databaseBuilder)
        : base(discordClient, mapper, databaseBuilder)
    {
    }

    public Task<Data.Models.API.Users.GuildUser?> GetGuildUserAsync(ulong guildId, ulong userId)
    {
        return GetMappedEntityAsync(
            Tuple.Create(guildId, userId),
            async () =>
            {
                var guild = await DiscordClient.GetGuildAsync(guildId, CacheMode.CacheOnly);
                return guild is null ? null : await guild.GetUserAsync(userId, CacheMode.CacheOnly);
            },
            repo => repo.GuildUser.FindGuildUserByIdAsync(guildId, userId, true)
        );
    }
}
