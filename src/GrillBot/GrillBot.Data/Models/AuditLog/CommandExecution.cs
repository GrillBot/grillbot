using Discord;
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

    public CommandExecution() { }

    public CommandExecution(CommandInfo command, IMessage message, IResult result, int duration)
    {
        Command = command.Aliases[0];
        MessageContent = message.Content;
        Duration = duration;

        if (result == null) 
            return;

        IsSuccess = result.IsSuccess;
        CommandError = result.Error;
        ErrorReason = result.ErrorReason;
    }
}
