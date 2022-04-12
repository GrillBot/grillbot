using GrillBot.App.Infrastructure;

namespace GrillBot.App.Services.Emotes;

[Initializable]
public class EmotesCacheService : ServiceBase
{
    private ConcurrentBag<Tuple<GuildEmote, IGuild>> SupportedEmotes { get; } = new();
    private readonly object SupportedEmotesLock = new();

    public EmotesCacheService(DiscordSocketClient client) : base(client)
    {
        DiscordClient.Ready += () => SyncSupportedEmotesAsync();
        DiscordClient.GuildAvailable += _ => SyncSupportedEmotesAsync();
        DiscordClient.GuildUpdated += (_, _) => SyncSupportedEmotesAsync();
    }

    private Task SyncSupportedEmotesAsync()
    {
        lock (SupportedEmotesLock)
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
        lock (SupportedEmotesLock)
        {
            return SupportedEmotes.ToList();
        }
    }
}
