using GrillBot.Core.Models;

namespace GrillBot.Common.Services.AuditLog.Models.Response.Detail;

public class ThreadUpdatedDetail
{
    public Diff<List<string>>? Tags { get; set; } = new();
}
