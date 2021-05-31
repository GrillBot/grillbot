using Discord;
using Discord.Commands;
using Newtonsoft.Json;
using System.Linq;

namespace GrillBot.Data.Models.AuditLog
{
    public class CommandExecution
    {
        [JsonProperty("cmd")]
        public string Command { get; set; }

        [JsonProperty("content")]
        public string MessageContent { get; set; }

        [JsonProperty("success")]
        public bool IsSuccess { get; set; }

        [JsonProperty("err_type")]
        public CommandError? CommandError { get; set; }

        [JsonProperty("err")]
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
