using GrillBot.Common.Managers.Events.Contracts;

namespace GrillBot.App.Handlers.UserJoined;

public class UserJoinedSyncHandler : IUserJoinedEvent
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }

    public UserJoinedSyncHandler(GrillBotDatabaseBuilder databaseBuilder)
    {
        DatabaseBuilder = databaseBuilder;
    }

    public async Task ProcessAsync(IGuildUser user)
    {
        await using var repository = DatabaseBuilder.CreateRepository();

        var guildUser = await repository.GuildUser.FindGuildUserAsync(user);
        if (guildUser == null) return;

        await repository.CommitAsync();
    }
}
