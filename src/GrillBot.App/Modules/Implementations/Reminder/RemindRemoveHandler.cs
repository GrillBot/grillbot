using GrillBot.App.Infrastructure;

namespace GrillBot.App.Modules.Implementations.Reminder;

public class RemindRemoveHandler : ComponentInteractionHandler
{
    public override async Task ProcessAsync(IInteractionContext context)
    {
        if (!context.Interaction.IsDMInteraction)
            return;
        if (!TryParseMesasge(context.Interaction, out var message))
        {
            await context.Interaction.DeferAsync();
            return;
        }

        await context.Interaction.DeferAsync();
        await message.DeleteAsync();
    }
}
