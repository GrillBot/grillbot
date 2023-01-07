using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Managers.Events.Contracts;

namespace GrillBot.App.Handlers.UserUpdated;

public class SyncUserUpdatedHandler : IUserUpdatedEvent
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }

    public SyncUserUpdatedHandler(GrillBotDatabaseBuilder databaseBuilder)
    {
        DatabaseBuilder = databaseBuilder;
    }

    public async Task ProcessAsync(IUser before, IUser after)
    {
        if (!CanProcess(before, after)) return;

        await using var repository = DatabaseBuilder.CreateRepository();

        var user = await repository.User.FindUserAsync(after);
        if (user == null) return;

        await repository.CommitAsync();
    }

    private static bool CanProcess(IUser before, IUser after)
        => before.Username != after.Username || before.Discriminator != after.Discriminator || before.IsUser() != after.IsUser();
}
