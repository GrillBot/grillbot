using GrillBot.Data.Models.API.Guilds;

namespace GrillBot.Data.Models.API.Users;

public class GuildUserInfo
{
    public string Id { get; set; } = null!;
    public string Username { get; set; } = null!;
    public string? GlobalAlias { get; set; }
    public bool IsBot { get; set; }
    public string? AvatarUrl { get; set; }
    public Guild Guild { get; set; } = null!;

    public int UnverifyCount { get; set; }
    public int SelfUnverifyCount { get; set; }
    public int WarningCount { get; set; }
}
