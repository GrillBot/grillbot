namespace GrillBot.Common.Services.AuditLog.Models.Request.CreateItems;

public class MessageEditedRequest
{
    public string JumpUrl { get; set; } = null!;
    public string ContentBefore { get; set; } = null!;
    public string ContentAfter { get; set; } = null!;
}
