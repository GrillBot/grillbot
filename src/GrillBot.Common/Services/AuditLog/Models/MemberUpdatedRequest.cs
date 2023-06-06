using AuditLogService.Models.Request;

namespace GrillBot.Common.Services.AuditLog.Models;

public class MemberUpdatedRequest
{
    public string UserId { get; set; } = null!;
    public DiffRequest<string?>? SelfUnverifyMinimalTime { get; set; }
    public DiffRequest<int?>? Flags { get; set; }
}
