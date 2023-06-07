using Discord;
using GrillBot.Database.Entity;

namespace GrillBot.Data.Models.AuditLog;

public class AuditChannelInfo : AuditChannelBaseInfo
{
    public ChannelType? Type { get; set; }
    public bool? IsNsfw { get; set; }
    public int? Bitrate { get; set; }
    public string Topic { get; set; }
    public long Flags { get; set; }
    public int Position { get; set; } = -1;

    public AuditChannelInfo()
    {
    }

    public AuditChannelInfo(GuildChannel channel)
    {
        Flags = channel.Flags;
    }
}
