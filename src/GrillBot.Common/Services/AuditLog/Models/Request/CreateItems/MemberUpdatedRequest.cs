namespace GrillBot.Common.Services.AuditLog.Models.Request.CreateItems;

public class MemberUpdatedRequest
{
    public string UserId { get; set; } = null!;
    public DiffRequest<string?>? SelfUnverifyMinimalTime { get; set; }
    public DiffRequest<int?>? Flags { get; set; }
}
