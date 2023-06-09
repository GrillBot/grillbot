namespace GrillBot.Common.Services.AuditLog.Models.Response.Search;

public class InteractionCommandPreview
{
    public string Name { get; set; } = null!;
    public bool HasResponded { get; set; }
    public bool IsSuccess { get; set; }
}
