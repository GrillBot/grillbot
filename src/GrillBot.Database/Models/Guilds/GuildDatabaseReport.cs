﻿namespace GrillBot.Database.Models.Guilds;

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
    public long EmoteStats { get; set; }
    public int PointTransactions { get; set; }
    public int UserMeasures { get; set; }
}
