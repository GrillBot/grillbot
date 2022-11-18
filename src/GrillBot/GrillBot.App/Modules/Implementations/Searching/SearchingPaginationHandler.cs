using System.Diagnostics.CodeAnalysis;
using GrillBot.App.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace GrillBot.App.Modules.Implementations.Searching;

[ExcludeFromCodeCoverage]
public class SearchingPaginationHandler : ComponentInteractionHandler
{
    private int Page { get; }
    private IServiceProvider ServiceProvider { get; }

    public SearchingPaginationHandler(IServiceProvider serviceProvider, int page)
    {
        Page = page;
        ServiceProvider = serviceProvider;
    }

    public override async Task ProcessAsync(IInteractionContext context)
    {
        if (!TryParseData<SearchingMetadata>(context.Interaction, out var component, out var metadata))
        {
            await context.Interaction.DeferAsync();
            return;
        }

        var channel = await context.Guild.GetTextChannelAsync(metadata.ChannelId);
        if (channel == null)
        {
            await context.Interaction.DeferAsync();
            return;
        }

        using var scope = ServiceProvider.CreateScope();

        var command = scope.ServiceProvider.GetRequiredService<Actions.Commands.Searching.GetSearchingList>();
        command.Init(context);

        var pagesCount = await command.ComputePagesCountAsync(metadata.MessageQuery, channel);
        var newPage = CheckNewPageNumber(Page, pagesCount);
        if (newPage == metadata.Page)
        {
            await context.Interaction.DeferAsync();
            return;
        }

        var (embed, paginationComponent) = await command.ProcessAsync(newPage, metadata.MessageQuery, channel);

        await component.UpdateAsync(msg =>
        {
            msg.Components = paginationComponent;
            msg.Embed = embed;
        });
    }
}
