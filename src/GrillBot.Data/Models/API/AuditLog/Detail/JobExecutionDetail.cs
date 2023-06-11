using System;
using GrillBot.Data.Models.API.Users;

namespace GrillBot.Data.Models.API.AuditLog.Detail;

public class JobExecutionDetail
{
    public string JobName { get; set; } = null!;
    public string Result { get; set; } = null!;
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }
    public bool WasError { get; set; }
    public User? StartUser { get; set; }
}
