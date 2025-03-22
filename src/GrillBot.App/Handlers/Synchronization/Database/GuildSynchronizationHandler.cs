using GrillBot.Common.Managers.Events.Contracts;
using GrillBot.Core.Extensions;
using GrillBot.Database.Services.Repository;

namespace GrillBot.App.Handlers.Synchronization.Database;

public class GuildSynchronizationHandler : BaseSynchronizationHandler, IGuildAvailableEvent, IGuildUpdatedEvent, IJoinedGuildEvent
{
    public GuildSynchronizationHandler(GrillBotDatabaseBuilder databaseBuilder) : base(databaseBuilder)
    {
    }

    // GuildAvailable
    public async Task ProcessAsync(IGuild guild)
    {
        using var repository = CreateRepository();
        await repository.Guild.GetOrCreateGuildAsync(guild);

        await repository.CommitAsync();
    }

    // GuildUpdated
    public async Task ProcessAsync(IGuild before, IGuild after)
    {
        using var repository = CreateRepository();
        await ProcessCommonGuildChangesAsync(before, after, repository);

        await repository.CommitAsync();
    }

    private static async Task ProcessCommonGuildChangesAsync(IGuild before, IGuild after, GrillBotRepository repository)
    {
        if (before.Name == after.Name && before.Roles.Select(o => o.Id).IsSequenceEqual(after.Roles.Select(o => o.Id)))
            return;

        await repository.Guild.GetOrCreateGuildAsync(after);
    }
}
