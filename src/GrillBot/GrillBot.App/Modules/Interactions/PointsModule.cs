using Discord.Interactions;
using GrillBot.App.Infrastructure.Commands;
using GrillBot.App.Infrastructure.Preconditions.Interactions;
using GrillBot.App.Services.User.Points;
using GrillBot.Data.Exceptions;

namespace GrillBot.App.Modules.Interactions;

[RequireUserPerms]
[Group("points", "Points")]
public class PointsModule : InteractionsModuleBase
{
    private PointsService PointsService { get; }

    public PointsModule(PointsService pointsService)
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
}
