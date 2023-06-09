namespace GrillBot.Common.Services.AuditLog.Models.Request.CreateItems;

public class ChannelInfoRequest
{
    public string? Topic { get; set; }
    public int Position { get; set; }
    public long Flags { get; set; }
}
