namespace GrillBot.Common.Services.AuditLog.Models.Response.Detail;

public class ApiRequestDetail
{
    public string ControllerName { get; set; } = null!;
    public string ActionName { get; set; } = null!;
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }
    public string TemplatePath { get; set; } = null!;
    public string Path { get; set; } = null!;
    public Dictionary<string, string> Parameters { get; set; } = new();
    public string Language { get; set; } = null!;
    public string ApiGroupName { get; set; } = null!;
    public Dictionary<string, string> Headers { get; set; } = new();
    public string Identification { get; set; } = null!;
    public string Ip { get; set; } = null!;
    public string Result { get; set; } = null!;
    public string? Role { get; set; }
    public string? ForwardedIp { get; set; }
}
