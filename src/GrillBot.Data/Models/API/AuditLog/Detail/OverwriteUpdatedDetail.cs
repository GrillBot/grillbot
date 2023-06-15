using System.Collections.Generic;
using GrillBot.Core.Models;
using GrillBot.Data.Models.API.Users;

namespace GrillBot.Data.Models.API.AuditLog.Detail;

public class OverwriteUpdatedDetail
{
    public User? User { get; set; }
    public Role? Role { get; set; }
    
    public Diff<List<string>>? Allow { get; set; }
    public Diff<List<string>>? Deny { get; set; }
}
