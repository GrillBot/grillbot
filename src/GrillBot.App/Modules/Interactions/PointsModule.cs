using Discord.Interactions;
using GrillBot.App.Actions.Commands.Points;
using GrillBot.App.Actions.Commands.Points.Chart;
using GrillBot.App.Infrastructure.Commands;
using GrillBot.App.Infrastructure.Preconditions.Interactions;
using GrillBot.App.Modules.Implementations.Points;
using GrillBot.Core.Exceptions;

namespace GrillBot.App.Modules.Interactions;

[RequireUserPerms]
[Group("points", "Points")]
public class PointsModule : InteractionsModuleBase
{
    public PointsModule(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    [UserCommand("Body uživatele")]
    [SlashCommand("where", "Get the current status of user points.")]
    public async Task GetUserPointsAsync(IUser? user = null)
    {
        using var command = GetCommand<PointsImage>();

        try
        {
            using var img = await command.Command.ProcessAsync(Context.Guild, user ?? Context.User);
            await FollowupWithFileAsync(img.Path);
        }
        catch (NotFoundException ex)
        {
            await SetResponseAsync(ex.Message);
        }
    }

    [SlashCommand("board", "Get leaderboard of the points.")]
    public async Task GetPointsBoardAsync()
    {
        using var command = GetCommand<PointsLeaderboard>();

        try
        {
            var (embed, paginationComponent) = await command.Command.ProcessAsync(0);

            await SetResponseAsync(embed: embed, components: paginationComponent);
        }
        catch (NotFoundException ex)
        {
            await SetResponseAsync(ex.Message);
        }
    }

    [RequireSameUserAsAuthor]
    [ComponentInteraction("points:*", ignoreGroupNames: true)]
    public async Task HandlePointsBoardPaginationAsync(int page)
    {
        var handler = new PointsBoardPaginationHandler(page, ServiceProvider);
        await handler.ProcessAsync(Context);
    }

    [SlashCommand("chart", "Get charts of points")]
    public async Task GetChartAsync(ChartType type, ChartsFilter filter, IEnumerable<IUser>? users = null)
    {
        using var command = GetCommand<PointsChart>();

        using var img = await command.Command.ProcessAsync(type, users, filter);
        await FollowupWithFileAsync(img.Path);
    }
}
