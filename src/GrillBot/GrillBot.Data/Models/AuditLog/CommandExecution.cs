using Discord.Commands;

namespace GrillBot.Data.Models.AuditLog;

public class CommandExecution
{
    public string Command { get; set; }
    public string MessageContent { get; set; }
    public bool IsSuccess { get; set; }
    public CommandError? CommandError { get; set; }
    public string ErrorReason { get; set; }
    public int Duration { get; set; }
    public string Exception { get; set; }
}
