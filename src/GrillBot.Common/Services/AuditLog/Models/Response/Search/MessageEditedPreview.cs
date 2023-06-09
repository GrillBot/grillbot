namespace GrillBot.Common.Services.AuditLog.Models.Response.Search;

public class MessageEditedPreview
{
    public string ContentBefore { get; set; } = null!;
    public string ContentAfter { get; set; } = null!;
    public string JumpUrl { get; set; } = null!;
}
