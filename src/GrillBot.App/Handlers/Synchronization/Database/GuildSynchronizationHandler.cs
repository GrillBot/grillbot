using GrillBot.Common.Managers.Events.Contracts;

namespace GrillBot.App.Handlers.Synchronization.Database;

public class GuildSynchronizationHandler : BaseSynchronizationHandler, IGuildAvailableEvent, IGuildUpdatedEvent, IJoinedGuildEvent
{
    public GuildSynchronizationHandler(GrillBotDatabaseBuilder databaseBuilder) : base(databaseBuilder)
    {
    }

    public async Task ProcessAsync(IGuild guild)
    {
        await using var repository = CreateRepository();

        await repository.Guild.GetOrCreateGuildAsync(guild);
        await repository.CommitAsync();
    }

    public async Task ProcessAsync(IGuild before, IGuild after)
    {
        if (before.Name == after.Name && before.Roles.Select(o => o.Id).OrderBy(o => o).SequenceEqual(after.Roles.Select(o => o.Id).OrderBy(o => o)))
            return;

        await using var repository = CreateRepository();

        await repository.Guild.GetOrCreateGuildAsync(after);
        await repository.CommitAsync();
    }
}
