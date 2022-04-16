namespace GrillBot.Data.Models.API.Guilds;

public class UpdateGuildParams
{
    public string MuteRoleId { get; set; }
    public string AdminChannelId { get; set; }
    public string EmoteSuggestionChannelId { get; set; }
}
