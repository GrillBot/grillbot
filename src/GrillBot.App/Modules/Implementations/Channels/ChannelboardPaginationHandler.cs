using GrillBot.App.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace GrillBot.App.Modules.Implementations.Channels;

public class ChannelboardPaginationHandler : ComponentInteractionHandler
{
    private int Page { get; }
    private IServiceProvider ServiceProvider { get; }

    public ChannelboardPaginationHandler(IServiceProvider serviceProvider, int page)
    {
        Page = page;
        ServiceProvider = serviceProvider;
    }

    public override async Task ProcessAsync(IInteractionContext context)
    {
        if (!TryParseData<ChannelboardMetadata>(context.Interaction, out var component, out var metadata))
        {
            await context.Interaction.DeferAsync();
            return;
        }

        using var scope = ServiceProvider.CreateScope();
        var command = scope.ServiceProvider.GetRequiredService<Actions.Commands.GetChannelboard>();
        command.Init(context);

        var pagesCount = await command.ComputePagesCountAsync();
        var newPage = CheckNewPageNumber(Page, pagesCount);
        if (newPage == metadata.Page)
        {
            await context.Interaction.DeferAsync();
            return;
        }

        var (embed, paginationComponents) = await command.ProcessAsync(newPage);
        await component.UpdateAsync(msg =>
        {
            msg.Components = paginationComponents;
            msg.Embed = embed;
        });
    }
}
