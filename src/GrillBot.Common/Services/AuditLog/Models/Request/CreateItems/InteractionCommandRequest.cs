namespace GrillBot.Common.Services.AuditLog.Models;

public class InteractionCommandRequest
{
    public string Name { get; set; } = null!;
    public string ModuleName { get; set; } = null!;
    public string MethodName { get; set; } = null!;
    public List<InteractionCommandParameterRequest> Parameters { get; set; } = new();
    public bool HasResponded { get; set; }
    public bool IsValidToken { get; set; }
    public bool IsSuccess { get; set; }
    public int? CommandError { get; set; }
    public string? ErrorReason { get; set; }
    public int Duration { get; set; }
    public string? Exception { get; set; }
    public string Locale { get; set; } = "cs";
}
