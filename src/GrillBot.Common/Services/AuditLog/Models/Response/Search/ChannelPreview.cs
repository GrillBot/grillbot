namespace GrillBot.Common.Services.AuditLog.Models.Response.Search;

public class ChannelPreview
{
    public string Name { get; set; } = null!;
    public string Type { get; set; } = null!;
    public int? Slowmode { get; set; }
    public bool? IsNsfw { get; set; }
    public int? Bitrate { get; set; }
}
