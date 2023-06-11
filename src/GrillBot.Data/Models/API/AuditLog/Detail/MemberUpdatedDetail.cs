using GrillBot.Core.Models;
using GrillBot.Data.Models.API.Users;

namespace GrillBot.Data.Models.API.AuditLog.Detail;

public class MemberUpdatedDetail
{
    public User User { get; set; } = null!;
    public Diff<string?>? Nickname { get; set; }
    public Diff<bool?>? IsMuted { get; set; }
    public Diff<bool?>? IsDeaf { get; set; }
    public Diff<string?>? SelfUnverifyMinimalTime { get; set; }
    public Diff<int?>? Flags { get; set; }
}
