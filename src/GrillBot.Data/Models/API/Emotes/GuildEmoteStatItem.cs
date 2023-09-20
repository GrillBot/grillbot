using GrillBot.Data.Models.API.Guilds;

namespace GrillBot.Data.Models.API.Emotes;

public class GuildEmoteStatItem : EmoteStatItem
{
    public Guild Guild { get; set; } = null!;
}
