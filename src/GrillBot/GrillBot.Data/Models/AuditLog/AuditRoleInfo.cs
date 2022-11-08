using Discord;

namespace GrillBot.Data.Models.AuditLog;

public class AuditRoleInfo
{
    public ulong Id { get; set; }
    public string RoleId { get; set; }
    
    public string Name { get; set; }
    public uint Color { get; set; }

    public AuditRoleInfo() { }

    public AuditRoleInfo(ulong id, string name, Color color)
    {
        RoleId = id.ToString();
        Name = name;
        Color = color.RawValue;
    }

    public AuditRoleInfo(IRole role) : this(role.Id, role.Name, role.Color) { }
}
