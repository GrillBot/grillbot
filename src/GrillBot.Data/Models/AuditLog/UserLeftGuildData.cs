namespace GrillBot.Data.Models.AuditLog;

public class UserLeftGuildData
{
    public int MemberCount { get; set; }
    public bool IsBan { get; set; }
    public string BanReason { get; set; }
    public AuditUserInfo User { get; set; }
}
