using GrillBot.App.Infrastructure;
using GrillBot.Core.Services.Common;
using GrillBot.Core.Services.RemindService;
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

        var remindServiceClient = ServiceProvider.GetRequiredService<IRemindServiceClient>();
        try
        {
            await remindServiceClient.PostponeRemindAsync(message.Id.ToString(), Hours);

            await context.Interaction.DeferAsync();
            await message.DeleteAsync();
        }
        catch (ClientNotFoundException)
        {
            await context.Interaction.DeferAsync();
        }
    }
}
