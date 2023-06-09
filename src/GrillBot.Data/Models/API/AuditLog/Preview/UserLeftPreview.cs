using GrillBot.Data.Models.API.Users;

namespace GrillBot.Data.Models.API.AuditLog.Preview;

public class UserLeftPreview
{
    public User User { get; set; } = null!;
    public int MemberCount { get; set; }
    public bool IsBan { get; set; }
    public string? BanReason { get; set; }
}
