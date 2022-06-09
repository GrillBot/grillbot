using Discord.Interactions;

namespace GrillBot.App.Infrastructure.Preconditions.Interactions;

public class RequireSameUserAsAuthorAttribute : PreconditionAttribute
{
    public override Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, ICommandInfo commandInfo, IServiceProvider services)
    {
        if (context.Interaction is not SocketMessageComponent component || component.Data.Type != ComponentType.Button)
            return Task.FromResult(PreconditionResult.FromSuccess()); // Do not check another types.

        if (context.User.Id == component.Message.Interaction.User.Id) 
            return Task.FromResult(PreconditionResult.FromSuccess());

        return Task.FromResult(PreconditionResult.FromError("Tuto akci může provést pouze původní autor příkazu."));
    }
}
