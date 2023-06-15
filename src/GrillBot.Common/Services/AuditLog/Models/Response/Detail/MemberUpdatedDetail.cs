using GrillBot.Core.Models;

namespace GrillBot.Common.Services.AuditLog.Models.Response.Detail;

public class MemberUpdatedDetail
{
    public string UserId { get; set; } = null!;
    public Diff<string?>? Nickname { get; set; }
    public Diff<bool?>? IsMuted { get; set; }
    public Diff<bool?>? IsDeaf { get; set; }
    public Diff<string?>? SelfUnverifyMinimalTime { get; set; }
    public Diff<int?>? Flags { get; set; }
}
