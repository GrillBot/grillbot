using System.Diagnostics.CodeAnalysis;
using GrillBot.App.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace GrillBot.App.Modules.Implementations.Reminder;

[ExcludeFromCodeCoverage]
public class RemindPaginationHandler : ComponentInteractionHandler
{
    private int Page { get; }
    private IServiceProvider ServiceProvider { get; }

    public RemindPaginationHandler(int page, IServiceProvider serviceProvider)
    {
        Page = page;
        ServiceProvider = serviceProvider;
    }

    public override async Task ProcessAsync(IInteractionContext context)
    {
        if (!TryParseData<RemindListMetadata>(context.Interaction, out var component, out var metadata))
        {
            await context.Interaction.DeferAsync();
            return;
        }

        using var scope = ServiceProvider.CreateScope();

        var action = scope.ServiceProvider.GetRequiredService<Actions.Commands.Reminder.GetReminderList>();
        action.Init(context);

        var pagesCount = await action.ComputePagesCountAsync();
        if (pagesCount == 0)
        {
            await context.Interaction.DeferAsync();
            return;
        }

        var newPage = CheckNewPageNumber(Page, pagesCount);
        if (newPage == metadata.Page)
        {
            await context.Interaction.DeferAsync();
            return;
        }

        var (embed, paginationComponent) = await action.ProcessAsync(Page);

        await component.UpdateAsync(msg =>
        {
            msg.Components = paginationComponent;
            msg.Embed = embed;
        });
    }
}
