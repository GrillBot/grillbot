namespace GrillBot.App.Services.Guild;

public class GuildEventsService
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }

    public GuildEventsService(GrillBotDatabaseBuilder databaseBuilder)
    {
        DatabaseBuilder = databaseBuilder;
    }

    public async Task<bool> ExistsValidGuildEventAsync(IGuild guild, string eventId)
    {
        await using var repository = DatabaseBuilder.CreateRepository();

        var dbGuild = await repository.Guild.FindGuildAsync(guild, true);
        var @event = dbGuild?.GuildEvents.FirstOrDefault(o => o.Id == eventId);
        if (@event == null) return false;

        var now = DateTime.Now;
        return now >= @event.From && now < @event.To;
    }
}
