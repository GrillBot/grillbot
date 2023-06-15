using GrillBot.Common.Services.AuditLog.Enums;

namespace GrillBot.Common.Services.AuditLog.Models.Response.Detail;

public class Detail
{
    public LogType Type { get; set; }
    public object? Data { get; set; }
}
