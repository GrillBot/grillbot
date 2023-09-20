using GrillBot.Common.Managers.Events.Contracts;

namespace GrillBot.App.Handlers.Ready;

public class EmoteInitSynchronizationHandler : IReadyEvent
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private IDiscordClient DiscordClient { get; }

    public EmoteInitSynchronizationHandler(GrillBotDatabaseBuilder databaseBuilder, IDiscordClient discordClient)
    {
        DatabaseBuilder = databaseBuilder;
        DiscordClient = discordClient;
    }

    public async Task ProcessAsync()
    {
        var guilds = await DiscordClient.GetGuildsAsync();
        var supportedEmotes = guilds.SelectMany(o => o.Emotes).Select(o => o.ToString()).ToHashSet();

        await using var repository = DatabaseBuilder.CreateRepository();

        var statistics = await repository.Emote.GetAllStatisticsAsync();
        foreach (var statistic in statistics)
        {
            statistic.IsEmoteSupported = supportedEmotes.Contains(statistic.EmoteId);
        }

        await repository.CommitAsync();
    }
}
