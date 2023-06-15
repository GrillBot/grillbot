using System.Collections.Generic;
using GrillBot.Data.Models.API.Users;

namespace GrillBot.Data.Models.API.AuditLog.Preview;

public class OverwritePreview
{
    public User? User { get; set; }
    public Role? Role { get; set; }

    public List<string> Allow { get; set; } = new();
    public List<string> Deny { get; set; } = new();
}
