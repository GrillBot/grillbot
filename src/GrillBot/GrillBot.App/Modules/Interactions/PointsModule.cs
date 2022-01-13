using Discord;
using Discord.Interactions;
using GrillBot.App.Services;
using GrillBot.Data.Exceptions;
using System.Threading.Tasks;

namespace GrillBot.App.Modules.Interactions;

public class PointsModule : Infrastructure.InteractionsModuleBase
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
            await ReplyFileAsync(img.Path, false, Context.User.Mention, noReply: true);
            await DeleteOriginalResponseAsync();
        }
        catch (NotFoundException ex)
        {
            await SetResponseAsync(ex.Message);
        }
    }
}
