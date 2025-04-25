using GrillBot.App.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace GrillBot.App.Modules.Implementations.Points;

public class PointsBoardPaginationHandler(
    int _page,
    IServiceProvider _serviceProvider
) : ComponentInteractionHandler
{
    public override async Task ProcessAsync(IInteractionContext context)
    {
        if (!TryParseData<PointsBoardMetadata>(context.Interaction, out var component, out var metadata))
        {
            await context.Interaction.DeferAsync();
            return;
        }

        using var scope = _serviceProvider.CreateScope();
        var action = scope.ServiceProvider.GetRequiredService<Actions.Commands.Points.PointsLeaderboard>();
        action.Init(context);

        var pagesCount = await action.ComputePagesCountAsync();
        var newPage = CheckNewPageNumber(_page, pagesCount);
        if (newPage == metadata.Page)
        {
            await context.Interaction.DeferAsync();
            return;
        }

        var (embed, paginationComponent) = await action.ProcessAsync(newPage, metadata.OverAllTime);
        await component.UpdateAsync(msg =>
        {
            msg.Components = paginationComponent;
            msg.Embed = embed;
        });
    }
}
