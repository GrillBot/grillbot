using GrillBot.Common.Extensions.Discord;
using GrillBot.Database.Enums;
using Microsoft.Extensions.Caching.Memory;

namespace GrillBot.App.Managers.DataResolve;

public class GuildUserResolver : BaseDataResolver<Tuple<ulong, ulong>, IGuildUser, Database.Entity.GuildUser, Data.Models.API.Users.GuildUser>
{
    public GuildUserResolver(IDiscordClient discordClient, GrillBotDatabaseBuilder databaseBuilder, IMemoryCache memoryCache)
        : base(discordClient, databaseBuilder, memoryCache)
    {
    }

    public Task<Data.Models.API.Users.GuildUser?> GetGuildUserAsync(ulong guildId, ulong userId)
    {
        return GetMappedEntityAsync(
            Tuple.Create(guildId, userId),
            async () =>
            {
                var guild = await _discordClient.GetGuildAsync(guildId, CacheMode.CacheOnly);
                return guild is null ? null : await guild.GetUserAsync(userId, CacheMode.CacheOnly);
            },
            repo => repo.GuildUser.FindGuildUserByIdAsync(guildId, userId, true)
        );
    }

    protected override Data.Models.API.Users.GuildUser Map(IGuildUser discordEntity)
    {
        return new Data.Models.API.Users.GuildUser
        {
            Id = discordEntity.Id.ToString(),
            GlobalAlias = discordEntity.GlobalName,
            AvatarUrl = discordEntity.GetUserAvatarUrl(128),
            IsBot = discordEntity.IsBot,
            Nickname = discordEntity.Nickname,
            Username = discordEntity.Username
        };
    }

    protected override Data.Models.API.Users.GuildUser Map(Database.Entity.GuildUser entity)
    {
        return new Data.Models.API.Users.GuildUser
        {
            Username = entity.User!.Username,
            Nickname = entity.Nickname,
            IsBot = entity.User.HaveFlags(UserFlags.NotUser),
            AvatarUrl = entity.User.AvatarUrl ?? CDN.GetDefaultUserAvatarUrl(0),
            Id = entity.UserId,
            GlobalAlias = entity.User!.GlobalAlias
        };
    }
}
