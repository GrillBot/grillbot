namespace GrillBot.Common.Services.AuditLog.Models.Response.Info.Dashboard;

public class DashboardInfo
{
    public List<DashboardInfoRow> InternalApi { get; set; } = new();
    public List<DashboardInfoRow> PublicApi { get; set; } = new();
    public List<DashboardInfoRow> Interactions { get; set; } = new();
    public List<DashboardInfoRow> Jobs { get; set; } = new();
    public TodayAvgTimes TodayAvgTimes { get; set; } = null!;
}
