using Discord;
using Discord.Commands;
using Newtonsoft.Json;
using System.Linq;

namespace GrillBot.Data.Models.AuditLog
{
    public class CommandExecution
    {
        public string Command { get; set; }
        public string MessageContent { get; set; }
        public bool IsSuccess { get; set; }
        public CommandError? CommandError { get; set; }
        public string ErrorReason { get; set; }

        public CommandExecution() { }

        public CommandExecution(CommandInfo command, IMessage message, IResult result)
        {
            Command = command.Aliases.FirstOrDefault();
            MessageContent = message.Content;

            if (result != null)
            {
                IsSuccess = result.IsSuccess;
                CommandError = result.Error;
                ErrorReason = result.ErrorReason;
            }
        }
    }
}
