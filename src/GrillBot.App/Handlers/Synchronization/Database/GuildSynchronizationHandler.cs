using GrillBot.Common.Managers.Events.Contracts;
using GrillBot.Core.Managers.Discord;
using GrillBot.Database.Services.Repository;

namespace GrillBot.App.Handlers.Synchronization.Database;

public class GuildSynchronizationHandler : BaseSynchronizationHandler, IGuildAvailableEvent, IGuildUpdatedEvent, IJoinedGuildEvent
{
    private IEmoteManager EmoteManager { get; }

    public GuildSynchronizationHandler(GrillBotDatabaseBuilder databaseBuilder, IEmoteManager emoteManager) : base(databaseBuilder)
    {
        EmoteManager = emoteManager;
    }

    // GuildAvailable
    public async Task ProcessAsync(IGuild guild)
    {
        await using var repository = CreateRepository();
        await repository.Guild.GetOrCreateGuildAsync(guild);

        var guildEmotes = await EmoteManager.GetSupportedEmotesAsync();
        var supportedEmotes = guildEmotes.Select(o => o.ToString()).ToHashSet();

        var statistics = await repository.Emote.GetStatisticsOfGuildAsync(guild);
        foreach (var stat in statistics)
            stat.IsEmoteSupported = supportedEmotes.Contains(stat.EmoteId);

        await repository.CommitAsync();
    }

    // GuildUpdated
    public async Task ProcessAsync(IGuild before, IGuild after)
    {
        await using var repository = CreateRepository();

        await ProcessCommonGuildChangesAsync(before, after, repository);
        await ProcessEmoteChangesAsync(before, after, repository);

        await repository.CommitAsync();
    }

    private static async Task ProcessCommonGuildChangesAsync(IGuild before, IGuild after, GrillBotRepository repository)
    {
        if (before.Name == after.Name && before.Roles.Select(o => o.Id).OrderBy(o => o).SequenceEqual(after.Roles.Select(o => o.Id).OrderBy(o => o)))
            return;

        await repository.Guild.GetOrCreateGuildAsync(after);
    }

    private static async Task ProcessEmoteChangesAsync(IGuild before, IGuild after, GrillBotRepository repository)
    {
        var emotesBefore = before.Emotes.Select(o => o.ToString()).ToList();
        var emotesAfter = after.Emotes.Select(o => o.ToString()).ToHashSet();

        foreach (var emote in emotesBefore.Where(e => !emotesAfter.Contains(e)))
        {
            var statistics = await repository.Emote.FindStatisticsByEmoteIdAsync(emote);
            foreach (var stat in statistics)
                stat.IsEmoteSupported = false;
        }
    }
}
