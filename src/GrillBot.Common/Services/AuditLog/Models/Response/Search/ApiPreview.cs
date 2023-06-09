namespace GrillBot.Common.Services.AuditLog.Models.Response.Search;

public class ApiPreview
{
    public string Action { get; set; } = null!;
    public string Path { get; set; } = null!;
    public string Result { get; set; } = null!;
    public int Duration { get; set; }
}
