using GrillBot.Data.Models.API.Guilds;

namespace GrillBot.Data.Models.API.Emotes;

public class GuildEmoteItem : EmoteItem
{
    public Guild Guild { get; set; } = null!;
}
