using Discord.Interactions;
using GrillBot.App.Infrastructure.Commands;
using GrillBot.App.Infrastructure.Preconditions.Interactions;
using GrillBot.App.Services.User.Points;
using GrillBot.Data.Exceptions;

namespace GrillBot.App.Modules.Interactions;

[RequireUserPerms]
public class PointsModule : InteractionsModuleBase
{
    private PointsService PointsService { get; }

    public PointsModule(PointsService pointsService)
    {
        PointsService = pointsService;
    }

    [UserCommand("Body uživatele")]
    public async Task GetUserPointsAsync(IUser user)
    {
        try
        {
            using var img = await PointsService.GetPointsOfUserImageAsync(Context.Guild, user);
            await FollowupWithFileAsync(img.Path);
        }
        catch (NotFoundException ex)
        {
            await SetResponseAsync(ex.Message);
        }
    }
}
