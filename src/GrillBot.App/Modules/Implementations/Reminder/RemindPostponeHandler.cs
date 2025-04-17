using GrillBot.App.Infrastructure;
using GrillBot.Core.Services.Common.Exceptions;
using GrillBot.Core.Services.Common.Executor;
using GrillBot.Core.Services.RemindService;
using Microsoft.Extensions.DependencyInjection;

namespace GrillBot.App.Modules.Implementations.Reminder;

public class RemindPostponeHandler(
    int _hours,
    IServiceProvider _serviceProvider
) : ComponentInteractionHandler
{
    public override async Task ProcessAsync(IInteractionContext context)
    {
        if (!context.Interaction.IsDMInteraction) return;
        if (!TryParseMesasge(context.Interaction, out var message))
        {
            await context.Interaction.DeferAsync();
            return;
        }

        var remindServiceClient = _serviceProvider.GetRequiredService<IServiceClientExecutor<IRemindServiceClient>>();
        try
        {
            await remindServiceClient.ExecuteRequestAsync((c, ctx) => c.PostponeRemindAsync(message.Id.ToString(), _hours, ctx.CancellationToken));
            await context.Interaction.DeferAsync();
            await message.DeleteAsync();
        }
        catch (ClientNotFoundException)
        {
            await context.Interaction.DeferAsync();
        }
    }
}
