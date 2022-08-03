using GrillBot.Database.Entity;

namespace GrillBot.Database.Models.Points;

public class PointBoardItem
{
    public string GuildId { get; set; } = null!;
    public string UserId { get; set; } = null!;
    
    public GuildUser GuildUser { get; set; } = null!;
    public long Points { get; set; }
}
