namespace GrillBot.Data.Models.API.Guilds;

public class GuildDatabaseReport
{
    public int Users { get; set; }
    public int Invites { get; set; }
    public int Channels { get; set; }
    public int Searches { get; set; }
    public int Unverifies { get; set; }
    public int UnverifyLogs { get; set; }
    public int AuditLogs { get; set; }
    public int CacheIndexes { get; set; }
}
