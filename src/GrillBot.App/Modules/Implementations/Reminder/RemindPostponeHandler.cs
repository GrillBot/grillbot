using GrillBot.App.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace GrillBot.App.Modules.Implementations.Reminder;

public class RemindPostponeHandler : ComponentInteractionHandler
{
    private int Hours { get; }
    private IServiceProvider ServiceProvider { get; }

    public RemindPostponeHandler(int hours, IServiceProvider serviceProvider)
    {
        Hours = hours;
        ServiceProvider = serviceProvider;
    }

    public override async Task ProcessAsync(IInteractionContext context)
    {
        if (!context.Interaction.IsDMInteraction) return;
        if (!TryParseMesasge(context.Interaction, out var message))
        {
            await context.Interaction.DeferAsync();
            return;
        }

        var builder = ServiceProvider.GetRequiredService<GrillBotDatabaseBuilder>();
        await using var repository = builder.CreateRepository();

        var remind = await repository.Remind.FindRemindByRemindMessageAsync(message.Id.ToString());
        if (remind == null)
        {
            await context.Interaction.DeferAsync();
            return;
        }

        remind.RemindMessageId = null;
        remind.At = DateTime.Now.AddHours(Hours);
        remind.Postpone++;

        await context.Interaction.DeferAsync();
        await message.DeleteAsync();
        await repository.CommitAsync();
    }
}
