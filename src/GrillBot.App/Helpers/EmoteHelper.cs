namespace GrillBot.App.Helpers;

public class EmoteHelper
{
    private IDiscordClient DiscordClient { get; }

    public EmoteHelper(IDiscordClient discordClient)
    {
        DiscordClient = discordClient;
    }

    public async Task<List<GuildEmote>> GetSupportedEmotesAsync()
    {
        var guilds = await DiscordClient.GetGuildsAsync();
        return guilds.SelectMany(o => o.Emotes).ToList();
    }
}
