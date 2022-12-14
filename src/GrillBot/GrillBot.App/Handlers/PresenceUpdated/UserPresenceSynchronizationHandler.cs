using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Managers.Events.Contracts;

namespace GrillBot.App.Handlers.PresenceUpdated;

public class UserPresenceSynchronizationHandler : IPresenceUpdatedEvent
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }

    public UserPresenceSynchronizationHandler(GrillBotDatabaseBuilder databaseBuilder)
    {
        DatabaseBuilder = databaseBuilder;
    }

    public async Task ProcessAsync(IUser user, IPresence before, IPresence after)
    {
        if (!CanProcess(before, after)) return;

        await using var repository = DatabaseBuilder.CreateRepository();

        var userEntity = await repository.User.FindUserAsync(user);
        if (userEntity == null) return;

        userEntity.Status = after.GetStatus();
        await repository.CommitAsync();
    }

    private static bool CanProcess(IPresence before, IPresence after)
        => before != null && after != null && before.GetStatus() != after.GetStatus();
}
