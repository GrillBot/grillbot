using GrillBot.App.Infrastructure;

namespace GrillBot.App.Services.Emotes;

[Initializable]
public class EmotesCacheService
{
    private ConcurrentBag<Tuple<GuildEmote, IGuild>> SupportedEmotes { get; } = new();
    private readonly object _supportedEmotesLock = new();
    private DiscordSocketClient DiscordClient { get; }

    public EmotesCacheService(DiscordSocketClient client)
    {
        DiscordClient = client;

        DiscordClient.Ready += SyncSupportedEmotesAsync;
        DiscordClient.GuildAvailable += _ => SyncSupportedEmotesAsync();
        DiscordClient.GuildUpdated += (_, _) => SyncSupportedEmotesAsync();
    }

    private Task SyncSupportedEmotesAsync()
    {
        lock (_supportedEmotesLock)
        {
            SupportedEmotes.Clear();
            DiscordClient.Guilds
                .SelectMany(o => o.Emotes.Select(x => new Tuple<GuildEmote, IGuild>(x, o)))
                .Distinct()
                .ToList()
                .ForEach(o => SupportedEmotes.Add(o));
        }

        return Task.CompletedTask;
    }

    public List<Tuple<GuildEmote, IGuild>> GetSupportedEmotes()
    {
        lock (_supportedEmotesLock)
        {
            return SupportedEmotes.ToList();
        }
    }
}
