using GrillBot.Data.Models.API.Users;

namespace GrillBot.Data.Models.API.AuditLog.Preview;

public class OverwriteUpdatedPreview
{
    public User? User { get; set; }
    public Role? Role { get; set; }
}
