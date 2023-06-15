namespace GrillBot.Common.Services.AuditLog.Models.Response.Detail;

public class EmbedField
{
    public string Name { get; set; } = null!;
    public string Value { get; set; } = null!;
    public bool Inline { get; set; }
}
