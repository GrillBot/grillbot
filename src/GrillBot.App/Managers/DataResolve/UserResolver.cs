using GrillBot.Common.Extensions.Discord;
using GrillBot.Database.Enums;
using Microsoft.Extensions.Caching.Memory;

namespace GrillBot.App.Managers.DataResolve;

public class UserResolver : BaseDataResolver<ulong, IUser, Database.Entity.User, Data.Models.API.Users.User>
{
    public UserResolver(IDiscordClient discordClient, GrillBotDatabaseBuilder databaseBuilder, IMemoryCache memoryCache)
        : base(discordClient, databaseBuilder, memoryCache)
    {
    }

    public Task<Data.Models.API.Users.User?> GetUserAsync(ulong userId)
    {
        return GetMappedEntityAsync(
            userId,
            () => _discordClient.GetUserAsync(userId, CacheMode.CacheOnly),
            repo => repo.User.FindUserByIdAsync(userId, disableTracking: true)
        );
    }

    protected override Data.Models.API.Users.User Map(IUser discordEntity)
    {
        return new Data.Models.API.Users.User
        {
            Id = discordEntity.Id.ToString(),
            AvatarUrl = discordEntity.GetUserAvatarUrl(128),
            GlobalAlias = discordEntity.GlobalName,
            IsBot = discordEntity.IsBot,
            Username = discordEntity.Username
        };
    }

    protected override Data.Models.API.Users.User Map(Database.Entity.User entity)
    {
        return new Data.Models.API.Users.User
        {
            Username = entity.Username,
            IsBot = entity.HaveFlags(UserFlags.NotUser),
            AvatarUrl = entity.AvatarUrl ?? CDN.GetDefaultUserAvatarUrl(0),
            GlobalAlias = entity.GlobalAlias,
            Id = entity.Id
        };
    }
}
