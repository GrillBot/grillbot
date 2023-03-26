using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Managers.Events.Contracts;
using GrillBot.Common.Services.RubbergodService;

namespace GrillBot.App.Handlers.UserUpdated;

public class SyncRubbergodServiceUserHandler : IUserUpdatedEvent
{
    private IRubbergodServiceClient RubbergodServiceClient { get; }

    public SyncRubbergodServiceUserHandler(IRubbergodServiceClient rubbergodServiceClient)
    {
        RubbergodServiceClient = rubbergodServiceClient;
    }

    public async Task ProcessAsync(IUser before, IUser after)
    {
        await SyncRubbergodServiceAsync(before, after);
    }

    private async Task SyncRubbergodServiceAsync(IUser before, IUser after)
    {
        if (before.Username == after.Username && before.Discriminator == after.Discriminator && before.GetUserAvatarUrl() == after.GetUserAvatarUrl()) return;
        await RubbergodServiceClient.RefreshMemberAsync(after.Id);
    }
}
