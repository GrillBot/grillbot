using Discord;

namespace GrillBot.Common.Managers.Emotes;

public class CachedEmote
{
    public GuildEmote Emote { get; set; } = null!;
    public IGuild Guild { get; set; } = null!;
}
