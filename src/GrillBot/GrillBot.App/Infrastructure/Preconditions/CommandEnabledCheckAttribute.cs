using Discord.Commands;
using GrillBot.App.Extensions.Discord;
using System;
using System.Threading.Tasks;

namespace GrillBot.App.Infrastructure.Preconditions
{
    public class CommandEnabledCheckAttribute : PreconditionAttribute
    {
        public string ErrMessage { get; set; }

        public CommandEnabledCheckAttribute(string errorMessage)
        {
            ErrMessage = errorMessage;
        }

        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            if (command.IsCommandEnabled())
                return Task.FromResult(PreconditionResult.FromSuccess());

            return Task.FromResult(PreconditionResult.FromError(ErrMessage));
        }
    }
}
