using AutoMapper;

namespace GrillBot.App.Managers.DataResolve;

public class UserDataResolver : BaseDataResolver<ulong, IUser, Database.Entity.User, Data.Models.API.Users.User>
{
    public UserDataResolver(IDiscordClient discordClient, IMapper mapper, GrillBotDatabaseBuilder databaseBuilder)
        : base(discordClient, mapper, databaseBuilder)
    {
    }

    public Task<Data.Models.API.Users.User?> GetUserAsync(ulong userId)
    {
        return GetMappedEntityAsync(
            userId,
            () => DiscordClient.GetUserAsync(userId, CacheMode.CacheOnly),
            repo => repo.User.FindUserByIdAsync(userId, disableTracking: true)
        );
    }
}
