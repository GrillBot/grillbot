using Discord.Interactions;

namespace GrillBot.App.Infrastructure.Preconditions.Interactions;

public class RequireSameUserAsAuthorAttribute : PreconditionAttribute
{
    public override async Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, ICommandInfo commandInfo, IServiceProvider services)
    {
        if (context.Interaction is not SocketMessageComponent component || component.Data.Type != ComponentType.Button)
            return PreconditionResult.FromSuccess(); // Do not check another types.

        if (context.User.Id != component.Message.Interaction.User.Id)
        {
            await context.Interaction.DeferAsync();
            return PreconditionResult.FromError("Tuto metodu může provést pouze původní autor příkazu.");
        }

        return PreconditionResult.FromSuccess();
    }
}
