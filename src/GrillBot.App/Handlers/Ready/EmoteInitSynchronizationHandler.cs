using GrillBot.Common.Managers.Events.Contracts;
using GrillBot.Core.Managers.Discord;

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

        await using var repository = DatabaseBuilder.CreateRepository();

        var statistics = await repository.Emote.GetAllStatisticsAsync();
        foreach (var statistic in statistics)
        {
            statistic.IsEmoteSupported = false;
        }

        var statisticsPerGuild = statistics.GroupBy(o => o.GuildId).ToDictionary(o => o.Key, o => o.ToList());

        foreach (var guild in guilds)
        {
            if (!statisticsPerGuild.TryGetValue(guild.Id.ToString(), out var guildStats))
                continue;

            var supportedEmotes = guild.Emotes.Select(o => o.ToString()).ToHashSet();
            foreach (var stat in guildStats)
                stat.IsEmoteSupported = supportedEmotes.Contains(stat.EmoteId);
        }

        await repository.CommitAsync();
    }
}
