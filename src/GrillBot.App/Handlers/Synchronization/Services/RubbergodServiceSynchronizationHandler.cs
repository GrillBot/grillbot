using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Managers.Events.Contracts;
using GrillBot.Common.Services.RubbergodService;

namespace GrillBot.App.Handlers.Synchronization.Services;

public class RubbergodServiceSynchronizationHandler : BaseSynchronizationHandler<IRubbergodServiceClient>, IUserUpdatedEvent
{
    public RubbergodServiceSynchronizationHandler(IRubbergodServiceClient serviceClient, GrillBotDatabaseBuilder databaseBuilder) : base(serviceClient, databaseBuilder)
    {
    }

    public async Task ProcessAsync(IUser before, IUser after)
    {
        if (before.Username == after.Username && before.Discriminator == after.Discriminator && before.GetUserAvatarUrl() == after.GetUserAvatarUrl())
            return;

        await ServiceClient.RefreshMemberAsync(after.Id);
    }
}
