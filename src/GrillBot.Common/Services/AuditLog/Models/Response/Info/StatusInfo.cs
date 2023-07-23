namespace GrillBot.Common.Services.AuditLog.Models.Response.Info;

public class StatusInfo
{
    public int ItemsToArchive { get; set; }
    public int ItemsToProcess { get; set; }
    public int ItemsToDelete { get; set; }
}
