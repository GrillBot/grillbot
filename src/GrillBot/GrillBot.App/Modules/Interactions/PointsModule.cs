using Discord.Interactions;
using GrillBot.App.Actions.Commands;
using GrillBot.App.Infrastructure.Commands;
using GrillBot.App.Infrastructure.Preconditions.Interactions;
using GrillBot.App.Modules.Implementations.Points;
using GrillBot.App.Services.User.Points;
using GrillBot.Data.Exceptions;

namespace GrillBot.App.Modules.Interactions;

[RequireUserPerms]
[Group("points", "Points")]
public class PointsModule : InteractionsModuleBase
{
    private PointsService PointsService { get; }

    public PointsModule(PointsService pointsService, IServiceProvider serviceProvider) : base(serviceProvider)
    {
        PointsService = pointsService;
    }

    [UserCommand("Body uživatele")]
    [SlashCommand("where", "Get the current status of user points.")]
    public async Task GetUserPointsAsync(IUser user = null)
    {
        try
        {
            using var img = await PointsService.GetPointsOfUserImageAsync(Context.Guild, user ?? Context.User);
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
}
