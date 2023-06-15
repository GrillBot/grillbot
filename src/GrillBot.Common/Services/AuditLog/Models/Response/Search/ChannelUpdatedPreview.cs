namespace GrillBot.Common.Services.AuditLog.Models.Response.Search;

public class ChannelUpdatedPreview
{
    public bool Name { get; set; }
    public bool SlowMode { get; set; }
    public bool IsNsfw { get; set; }
    public bool Bitrate { get; set; }
    public bool Topic { get; set; }
    public bool Flags { get; set; }
    public bool Position { get; set; }
}
