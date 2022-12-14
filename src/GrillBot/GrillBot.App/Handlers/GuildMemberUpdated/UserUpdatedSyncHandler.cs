using GrillBot.Common.Managers.Events.Contracts;

namespace GrillBot.App.Handlers.GuildMemberUpdated;

public class UserUpdatedSyncHandler : IGuildMemberUpdatedEvent
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }

    public UserUpdatedSyncHandler(GrillBotDatabaseBuilder databaseBuilder)
    {
        DatabaseBuilder = databaseBuilder;
    }

    public async Task ProcessAsync(IGuildUser before, IGuildUser after)
    {
        if (!CanProcess(before, after)) return;

        await using var repository = DatabaseBuilder.CreateRepository();

        var guildUser = await repository.GuildUser.FindGuildUserAsync(after);
        if (guildUser == null) return;

        await repository.CommitAsync();
    }

    private static bool CanProcess(IGuildUser before, IGuildUser after)
        => before != null && (before.Nickname != after.Nickname || before.Username != after.Username || before.Discriminator != after.Discriminator);
}
