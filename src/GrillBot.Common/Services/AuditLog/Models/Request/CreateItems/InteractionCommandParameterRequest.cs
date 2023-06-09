namespace GrillBot.Common.Services.AuditLog.Models.Request.CreateItems;

public class InteractionCommandParameterRequest
{
    public string Name { get; set; } = null!;
    public string Type { get; set; } = null!;
    public string Value { get; set; } = null!;
}
