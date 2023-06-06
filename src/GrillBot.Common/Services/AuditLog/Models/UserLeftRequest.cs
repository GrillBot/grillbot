using AuditLogService.Models.Request;

namespace GrillBot.Common.Services.AuditLog.Models;

public class UserLeftRequest : UserJoinedRequest
{
    public string UserId { get; set; } = null!;
}
