namespace GrillBot.Common.Services.AuditLog.Models.Response.Search;

public class MemberRoleUpdatedPreview
{
    public string UserId { get; set; } = null!;
    public Dictionary<string, bool> ModifiedRoles { get; set; } = new();
}
