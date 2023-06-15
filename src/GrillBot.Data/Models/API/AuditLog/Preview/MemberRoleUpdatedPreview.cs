using System.Collections.Generic;
using GrillBot.Data.Models.API.Users;

namespace GrillBot.Data.Models.API.AuditLog.Preview;

public class MemberRoleUpdatedPreview
{
    public User User { get; set; } = null!;
    public Dictionary<string, bool> ModifiedRoles { get; set; } = new();
}
