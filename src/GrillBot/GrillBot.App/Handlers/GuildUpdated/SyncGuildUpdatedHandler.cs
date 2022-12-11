using GrillBot.Common.Managers.Events.Contracts;

namespace GrillBot.App.Handlers.GuildUpdated;

public class SyncGuildUpdatedHandler : IGuildUpdatedEvent
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }

    public SyncGuildUpdatedHandler(GrillBotDatabaseBuilder databaseBuilder)
    {
        DatabaseBuilder = databaseBuilder;
    }

    public async Task ProcessAsync(IGuild before, IGuild after)
    {
        if (!CanProcess(before, after)) return;

        await using var repository = DatabaseBuilder.CreateRepository();

        var guild = await repository.Guild.FindGuildAsync(after);
        if (guild == null) return;

        await repository.CommitAsync();
    }

    private static bool CanProcess(IGuild before, IGuild after)
        => before.Name != after.Name || !before.Roles.SequenceEqual(after.Roles);
}
