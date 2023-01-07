using GrillBot.App.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace GrillBot.App.Modules.Implementations.Unverify;

public class UnverifyListPaginationHandler : ComponentInteractionHandler
{
    private int Page { get; }
    private IServiceProvider ServiceProvider { get; }

    public UnverifyListPaginationHandler(int page, IServiceProvider serviceProvider)
    {
        Page = page;
        ServiceProvider = serviceProvider;
    }

    public override async Task ProcessAsync(IInteractionContext context)
    {
        if (!TryParseData<UnverifyListMetadata>(context.Interaction, out var component, out var metadata))
        {
            await context.Interaction.DeferAsync();
            return;
        }

        using var scope = ServiceProvider.CreateScope();
        var action = scope.ServiceProvider.GetRequiredService<Actions.Commands.Unverify.UnverifyList>();
        action.Init(context);

        var pagesCount = await action.ComputeCountOfUnverifies();
        var page = CheckNewPageNumber(Page, pagesCount);
        if (page == metadata.Page)
        {
            await context.Interaction.DeferAsync();
            return;
        }

        var (embed, paginationComponent) = await action.ProcessAsync(page);
        await component.UpdateAsync(msg =>
        {
            msg.Components = paginationComponent;
            msg.Embed = embed;
        });
    }
}
