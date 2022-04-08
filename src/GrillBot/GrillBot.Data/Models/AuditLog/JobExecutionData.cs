using System;

namespace GrillBot.Data.Models.AuditLog;

public class JobExecutionData
{
    public string JobName { get; set; }
    public string Result { get; set; }
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }
    public bool WasError { get; set; }

    public void MarkFinished()
    {
        EndAt = DateTime.Now;
    }
}
