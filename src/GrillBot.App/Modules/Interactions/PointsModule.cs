﻿using Discord.Interactions;
using GrillBot.App.Actions.Commands.Points;
using GrillBot.App.Actions.Commands.Points.Chart;
using GrillBot.App.Infrastructure;
using GrillBot.App.Infrastructure.Preconditions.Interactions;
using GrillBot.App.Modules.Implementations.Points;
using GrillBot.Core.Exceptions;

namespace GrillBot.App.Modules.Interactions;

[RequireUserPerms]
[Group("points", "Points")]
public class PointsModule(IServiceProvider serviceProvider) : InteractionsModuleBase(serviceProvider)
{
    [UserCommand("Body uživatele")]
    [SlashCommand("where", "Get the current status of user points.")]
    public async Task GetUserPointsAsync(IUser? user = null)
    {
        using var command = await GetCommandAsync<PointsImage>();

        try
        {
            using var img = await command.Command.ProcessAsync(Context.Guild, user ?? Context.User);
            await FollowupWithFileAsync(img.Path);
        }
        catch (Exception ex) when (ex is NotFoundException or InvalidOperationException)
        {
            await SetResponseAsync(ex.Message);
        }
    }

    [SlashCommand("board", "Get leaderboard of the points.")]
    public async Task GetPointsBoardAsync(
        bool overAllTime = false
    )
    {
        using var command = await GetCommandAsync<PointsLeaderboard>();

        try
        {
            var (embed, paginationComponent) = await command.Command.ProcessAsync(0, overAllTime);

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
        using var command = await GetCommandAsync<PointsChart>();

        using var img = await command.Command.ProcessAsync(type, users, filter);
        await FollowupWithFileAsync(img.Path);
    }
}
