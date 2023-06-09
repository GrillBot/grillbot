using GrillBot.Core.Models;

namespace GrillBot.Data.Models.AuditLog;

public class MessageEditedData
{
    public Diff<string> Diff { get; set; }
    public string JumpUrl { get; set; }
}
