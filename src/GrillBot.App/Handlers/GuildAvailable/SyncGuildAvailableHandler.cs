using GrillBot.Common.Managers.Events.Contracts;

namespace GrillBot.App.Handlers.GuildAvailable;

public class SyncGuildAvailableHandler : IGuildAvailableEvent
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }

    public SyncGuildAvailableHandler(GrillBotDatabaseBuilder databaseBuilder)
    {
        DatabaseBuilder = databaseBuilder;
    }

    public async Task ProcessAsync(IGuild guild)
    {
        await using var repository = DatabaseBuilder.CreateRepository();

        var guildEntity = await repository.Guild.FindGuildAsync(guild);
        if (guildEntity == null) return;

        await repository.CommitAsync();
    }
}
