using Discord.Commands;
using System;
using System.Threading.Tasks;

namespace GrillBot.App.Infrastructure.Preconditions
{
    public class RequireReferenceAttribute : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            if (context.Message.Reference == null)
                return Task.FromResult(PreconditionResult.FromError("Tento příkaz vyžaduje reply na zprávu."));

            return Task.FromResult(PreconditionResult.FromSuccess());
        }
    }
}
