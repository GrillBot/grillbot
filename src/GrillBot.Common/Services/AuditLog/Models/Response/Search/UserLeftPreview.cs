namespace GrillBot.Common.Services.AuditLog.Models.Response.Search;

public class UserLeftPreview
{
    public string UserId { get; set; } = null!;
    public int MemberCount { get; set; }
    public bool IsBan { get; set; }
    public string? BanReason { get; set; }
}
