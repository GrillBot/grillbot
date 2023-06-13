namespace GrillBot.Common.Services.AuditLog.Models.Response.Info.Dashboard;

public class DashboardInfoRow
{
    public string Name { get; set; } = null!;
    public int Duration { get; set; }
    public bool Success { get; set; }
    public string Result { get; set; } = null!;
}
