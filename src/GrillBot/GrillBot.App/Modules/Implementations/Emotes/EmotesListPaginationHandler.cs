using GrillBot.App.Infrastructure;
using GrillBot.Common.Extensions.Discord;
using Microsoft.Extensions.DependencyInjection;

namespace GrillBot.App.Modules.Implementations.Emotes;

public class EmotesListPaginationHandler : ComponentInteractionHandler
{
    private int Page { get; }
    private IServiceProvider ServiceProvider { get; }

    public EmotesListPaginationHandler(IServiceProvider serviceProvider, int page)
    {
        Page = page;
        ServiceProvider = serviceProvider;
    }

    public override async Task ProcessAsync(IInteractionContext context)
    {
        if (!TryParseData<EmoteListMetadata>(context.Interaction, out var component, out var metadata))
        {
            await context.Interaction.DeferAsync();
            return;
        }

        var ofUser = metadata.OfUserId == null ? null : await context.Client.FindUserAsync(metadata.OfUserId.Value);

        using var scope = ServiceProvider.CreateScope();
        var action = scope.ServiceProvider.GetRequiredService<Actions.Commands.Emotes.GetEmotesList>();
        action.Init(context);

        var pagesCount = await action.ComputePagesCountAsync(metadata.OrderBy, metadata.Descending, ofUser, metadata.FilterAnimated);
        var newPage = CheckNewPageNumber(Page, pagesCount);
        if (newPage == metadata.Page)
        {
            await context.Interaction.DeferAsync();
            return;
        }

        var (embed, paginationComponent) = await action.ProcessAsync(newPage, metadata.OrderBy, metadata.Descending, ofUser, metadata.FilterAnimated);
        await component.UpdateAsync(msg =>
        {
            msg.Components = paginationComponent;
            msg.Embed = embed;
        });
    }
}
