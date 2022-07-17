using GrillBot.Data.Infrastructure.Validation;

namespace GrillBot.Data.Models.API.Guilds;

public class UpdateGuildParams
{
    [DiscordId]
    public string MuteRoleId { get; set; }

    [DiscordId]
    public string AdminChannelId { get; set; }

    [DiscordId]
    public string EmoteSuggestionChannelId { get; set; }
    
    [DiscordId]
    public string VoteChannelId { get; set; }
}
