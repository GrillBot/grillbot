using GrillBot.Common.Managers.Events.Contracts;

namespace GrillBot.App.Handlers.JoinedGuild;

public class SyncJoinedGuildHandler : IJoinedGuildEvent
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }

    public SyncJoinedGuildHandler(GrillBotDatabaseBuilder databaseBuilder)
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
