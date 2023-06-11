namespace GrillBot.Common.Services.AuditLog.Models.Response.Detail;

public class InteractionCommandDetail
{
    public string FullName { get; set; } = null!;
    public List<InteractionCommandParameter> Parameters { get; set; } = new();
    public bool HasResponded { get; set; }
    public bool IsValidToken { get; set; }
    public bool IsSuccess { get; set; }
    public int? CommandError { get; set; }
    public string? ErrorReason { get; set; }
    public int Duration { get; set; }
    public string? Exception { get; set; }
    public string Locale { get; set; } = null!;
}
