using Discord.WebSocket;

namespace GrillBot.Common.Managers.Emotes;

public class EmoteCache : IEmoteCache
{
    private readonly object _lock = new();
    private List<CachedEmote> SupportedEmotes { get; set; }
    private DiscordSocketClient DiscordClient { get; }

    public EmoteCache(DiscordSocketClient client)
    {
        DiscordClient = client;
        SupportedEmotes = new List<CachedEmote>();

        DiscordClient.Ready += ReloadCacheAsync;
        DiscordClient.GuildAvailable += _ => ReloadCacheAsync();
        DiscordClient.GuildUpdated += (_, _) => ReloadCacheAsync();
        DiscordClient.GuildUnavailable += _ => ReloadCacheAsync();
    }

    private Task ReloadCacheAsync()
    {
        lock (_lock)
        {
            var newEmotes = DiscordClient.Guilds
                .SelectMany(g => g.Emotes.Select(e => new CachedEmote { Emote = e, Guild = g }))
                .ToList();

            SupportedEmotes = newEmotes;
        }

        return Task.CompletedTask;
    }

    public List<CachedEmote> GetEmotes()
    {
        lock (_lock)
        {
            return SupportedEmotes;
        }
    }
}
