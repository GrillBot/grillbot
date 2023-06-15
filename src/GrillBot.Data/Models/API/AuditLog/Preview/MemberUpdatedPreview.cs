using GrillBot.Data.Models.API.Users;

namespace GrillBot.Data.Models.API.AuditLog.Preview;

public class MemberUpdatedPreview
{
    public User User { get; set; } = null!;

    public bool NicknameChanged { get; set; }
    public bool VoiceMuteChanged { get; set; }
    public bool SelfUnverifyMinimalTimeChange { get; set; }
    public bool FlagsChanged { get; set; }
}
