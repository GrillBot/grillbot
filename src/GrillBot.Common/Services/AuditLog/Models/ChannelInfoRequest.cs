namespace GrillBot.Common.Services.AuditLog.Models;

public class ChannelInfoRequest
{
    public string? Topic { get; set; }
    public int Position { get; set; }
    public long Flags { get; set; } 
}
